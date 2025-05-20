using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;

namespace RestaurantAppSQLSERVER.ViewModels
{
    public class AllergenCrudViewModel : ViewModelBase
    {
        public ObservableCollection<Allergen> Allergens { get; set; }
        private Allergen _selectedAllergen;
        public Allergen SelectedAllergen
        {
            get => _selectedAllergen;
            set
            {
                _selectedAllergen = value;
                OnPropertyChanged(nameof(SelectedAllergen));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)EditAllergenCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteAllergenCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveAllergenCommand).RaiseCanExecuteChanged();
            }
        }
        private string _allergenName;
        public string AllergenName
        {
            get => _allergenName;
            set
            {
                _allergenName = value;
                if (CurrentAllergenForEdit != null)
                {
                    CurrentAllergenForEdit.Name = value;
                }
                OnPropertyChanged(nameof(AllergenName));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveAllergenCommand).RaiseCanExecuteChanged();
            }
        }
        private Allergen _currentAllergenForEdit;
        public Allergen CurrentAllergenForEdit
        {
            get => _currentAllergenForEdit;
            set
            {
                _currentAllergenForEdit = value;
                AllergenName = value?.Name ?? string.Empty;
                OnPropertyChanged(nameof(CurrentAllergenForEdit));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveAllergenCommand).RaiseCanExecuteChanged();
            }
        }
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)EditAllergenCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteAllergenCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveAllergenCommand).RaiseCanExecuteChanged();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged(nameof(SuccessMessage));
            }
        }
        public ICommand LoadAllergensCommand { get; }
        public ICommand AddNewAllergenCommand { get; }
        public ICommand EditAllergenCommand { get; }
        public ICommand DeleteAllergenCommand { get; }
        public ICommand SaveAllergenCommand { get; }
        public ICommand CancelEditCommand { get; }

        private readonly AllergenService _allergenService;
        public AllergenCrudViewModel() : this(null)
        {
        }
        public AllergenCrudViewModel(AllergenService allergenService)
        {
            _allergenService = allergenService ?? throw new ArgumentNullException(nameof(allergenService));
            Allergens = new ObservableCollection<Allergen>();
            LoadAllergensCommand = new RelayCommand(async (param) => await ExecuteLoadAllergens());
            AddNewAllergenCommand = new RelayCommand(ExecuteAddNewAllergen);
            EditAllergenCommand = new RelayCommand(ExecuteEditAllergen, CanExecuteEditOrDeleteAllergen);
            DeleteAllergenCommand = new RelayCommand(async (param) => await ExecuteDeleteAllergen(), CanExecuteEditOrDeleteAllergen);
            SaveAllergenCommand = new RelayCommand(async (param) => await ExecuteSaveAllergen(), CanExecuteSaveAllergen);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);
            Task.Run(async () => await ExecuteLoadAllergens());

            AllergenName = string.Empty;
        }

        private async Task ExecuteLoadAllergens()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                if (_allergenService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real data.");
                    return;
                }

                var allergensList = await _allergenService.GetAllAllergensAsync();
                Allergens.Clear();
                foreach (var allergen in allergensList)
                {
                    Allergens.Add(allergen);
                }
                SuccessMessage = $"Au fost incarcati {Allergens.Count} alergeni.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea alergenilor: {ex.Message}";
            }
        }

        private void ExecuteAddNewAllergen(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            CurrentAllergenForEdit = new Allergen { Name = string.Empty };
            IsEditing = true;
        }

        private void ExecuteEditAllergen(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedAllergen != null)
            {
                CurrentAllergenForEdit = new Allergen
                {
                    Id = SelectedAllergen.Id,
                    Name = SelectedAllergen.Name
                };
                IsEditing = true;
            }
        }
        private bool CanExecuteEditOrDeleteAllergen(object parameter)
        {

            bool canExecute = SelectedAllergen != null && !IsEditing;
            Debug.WriteLine($"CanExecuteEditOrDeleteAllergen: SelectedAllergen is null? {SelectedAllergen == null}, IsEditing={IsEditing}, Result={canExecute}");

            return canExecute;
        }

        private async Task ExecuteDeleteAllergen()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedAllergen != null)
            {
                try
                {
                    if (_allergenService == null)
                    {
                        Debug.WriteLine("Running in design-time context, cannot delete real data.");
                        SelectedAllergen = null;
                        return;
                    }

                    await _allergenService.DeleteAllergenAsync(SelectedAllergen.Id);
                    Allergens.Remove(SelectedAllergen);
                    SelectedAllergen = null;
                    SuccessMessage = "Alergenul a fost sters cu succes.";
                }
                catch (InvalidOperationException ex)
                {
                    ErrorMessage = $"Eroare: {ex.Message}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Eroare la stergerea alergenului: {ex.Message}";
                }
            }
        }

        private async Task ExecuteSaveAllergen()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(AllergenName))
            {
                ErrorMessage = "Numele alergenului este obligatoriu.";
                return;
            }

            try
            {
                if (CurrentAllergenForEdit != null)
                {
                    CurrentAllergenForEdit.Name = AllergenName;
                }

                if (CurrentAllergenForEdit.Id == 0)
                {
                    await _allergenService.AddAllergenAsync(CurrentAllergenForEdit);
                    Allergens.Add(CurrentAllergenForEdit);
                    SuccessMessage = "Alergenul a fost adaugat cu succes.";
                }
                else
                {
                    await _allergenService.UpdateAllergenAsync(CurrentAllergenForEdit);
                    var existingAllergen = Allergens.FirstOrDefault(a => a.Id == CurrentAllergenForEdit.Id);
                    if (existingAllergen != null)
                    {
                        existingAllergen.Name = CurrentAllergenForEdit.Name;
                    }
                    SuccessMessage = "Alergenul a fost actualizat cu succes.";
                }

                IsEditing = false;
                SelectedAllergen = null;
                CurrentAllergenForEdit = null;
                AllergenName = string.Empty;
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = $"Eroare: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la salvarea alergenului: {ex.Message}";
            }
        }
        private bool CanExecuteSaveAllergen(object parameter)
        {
            bool canExecute = IsEditing && CurrentAllergenForEdit != null && !string.IsNullOrWhiteSpace(AllergenName);
            Debug.WriteLine($"CanExecuteSaveAllergen: IsEditing={IsEditing}, CurrentAllergenForEdit is null? {CurrentAllergenForEdit == null}, AllergenName='{AllergenName}', Result={canExecute}");
            return canExecute;
        }

        private void ExecuteCancelEdit(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsEditing = false;
            CurrentAllergenForEdit = null;
            AllergenName = string.Empty;
            SelectedAllergen = null;
        }
    }
}
