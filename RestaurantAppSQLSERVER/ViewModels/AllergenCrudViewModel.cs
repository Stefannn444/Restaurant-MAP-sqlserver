using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input; // For ICommand and CommandManager
using System.Linq;
using System.Diagnostics; // Adauga acest using pentru Debug.WriteLine

namespace RestaurantAppSQLSERVER.ViewModels
{
    // ViewModel pentru gestionarea operatiilor CRUD pe entitatea Allergen
    public class AllergenCrudViewModel : ViewModelBase
    {
        // Colectie pentru lista de alergeni afisati
        public ObservableCollection<Allergen> Allergens { get; set; }

        // Proprietate pentru selectia curenta in DataGrid
        private Allergen _selectedAllergen;
        public Allergen SelectedAllergen
        {
            get => _selectedAllergen;
            set
            {
                _selectedAllergen = value;
                OnPropertyChanged(nameof(SelectedAllergen));
                // Actualizeaza starea butoanelor Edit/Delete cand selectia se schimba
                CommandManager.InvalidateRequerySuggested(); // Notificare globala (standard)

                // Notificare EXPLICITA pentru command-urile Edit si Delete
                // Aceasta este adaugata pentru a ne asigura ca butoanele sunt re-evaluate
                ((RelayCommand)EditAllergenCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteAllergenCommand).RaiseCanExecuteChanged();

                // Notificare si pentru SaveCommand (desi nu se aplica direct la selectie, e bine sa fie consistenta)
                ((RelayCommand)SaveAllergenCommand).RaiseCanExecuteChanged();
            }
        }

        // Allergen name for two-way binding
        private string _allergenName;
        public string AllergenName
        {
            get => _allergenName;
            set
            {
                _allergenName = value;
                // Update the actual allergen object
                if (CurrentAllergenForEdit != null)
                {
                    CurrentAllergenForEdit.Name = value;
                }
                OnPropertyChanged(nameof(AllergenName));
                // Force re-evaluation of command states AFTER property change notification
                CommandManager.InvalidateRequerySuggested();
                // Explicitly raise CanExecuteChanged for SaveCommand
                ((RelayCommand)SaveAllergenCommand).RaiseCanExecuteChanged();
            }
        }

        // Proprietate pentru obiectul Allergen folosit pentru editare/adaugare in formular
        private Allergen _currentAllergenForEdit;
        public Allergen CurrentAllergenForEdit
        {
            get => _currentAllergenForEdit;
            set
            {
                _currentAllergenForEdit = value;
                // Update the AllergenName property when CurrentAllergenForEdit changes
                AllergenName = value?.Name ?? string.Empty;
                OnPropertyChanged(nameof(CurrentAllergenForEdit)); // Corrected nameof
                // Notifica CommandManager cand obiectul de editare se schimba
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveAllergenCommand).RaiseCanExecuteChanged(); // Explicitly raise CanExecuteChanged for SaveCommand
            }
        }

        // Starea UI-ului (ex: vizualizare lista, editare, adaugare)
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
                // Notifica CommandManager cand starea de editare se schimba
                CommandManager.InvalidateRequerySuggested();
                // Notificare EXPLICITA pentru command-urile Edit, Delete si Save
                // Acestea sunt afectate direct de starea IsEditing
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

        // Command-uri pentru operatiile CRUD
        public ICommand LoadAllergensCommand { get; }
        public ICommand AddNewAllergenCommand { get; }
        public ICommand EditAllergenCommand { get; }
        public ICommand DeleteAllergenCommand { get; }
        public ICommand SaveAllergenCommand { get; }
        public ICommand CancelEditCommand { get; }

        private readonly AllergenService _allergenService;

        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        public AllergenCrudViewModel() : this(null)
        {
        }

        // Constructorul principal - folosit la RULARE
        public AllergenCrudViewModel(AllergenService allergenService)
        {
            _allergenService = allergenService ?? throw new ArgumentNullException(nameof(allergenService));

            // Initializeaza colectia
            Allergens = new ObservableCollection<Allergen>();

            // Initializeaza command-urile
            LoadAllergensCommand = new RelayCommand(async (param) => await ExecuteLoadAllergens());
            AddNewAllergenCommand = new RelayCommand(ExecuteAddNewAllergen);
            EditAllergenCommand = new RelayCommand(ExecuteEditAllergen, CanExecuteEditOrDeleteAllergen);
            DeleteAllergenCommand = new RelayCommand(async (param) => await ExecuteDeleteAllergen(), CanExecuteEditOrDeleteAllergen);
            SaveAllergenCommand = new RelayCommand(async (param) => await ExecuteSaveAllergen(), CanExecuteSaveAllergen);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);

            // Initializeaza AllergenName cu un string gol
            Task.Run(async () => await ExecuteLoadAllergens()); // Poti incarca si lista de preparate aici

            AllergenName = string.Empty;
        }

        // --- Metode pentru Command-uri ---

        private async Task ExecuteLoadAllergens()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                // Verifica daca serviciul este null (cazul design-time)
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
            // Creeaza o instanta noua pentru adaugare si seteaza proprietatile
            CurrentAllergenForEdit = new Allergen { Name = string.Empty }; // Initializeaza Name
            IsEditing = true; // Trece in modul editare/adaugare
                              // The setters for CurrentAllergenForEdit and IsEditing will trigger CommandManager.InvalidateRequerySuggested() and explicit RaiseCanExecuteChanged
        }

        private void ExecuteEditAllergen(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedAllergen != null)
            {
                // Creeaza o copie a obiectului selectat pentru editare
                CurrentAllergenForEdit = new Allergen
                {
                    Id = SelectedAllergen.Id,
                    Name = SelectedAllergen.Name // Copiaza numele
                };
                IsEditing = true; // Trece in modul editare
                                  // The setters for CurrentAllergenForEdit and IsEditing will trigger CommandManager.InvalidateRequerySuggested() and explicit RaiseCanExecuteChanged
            }
        }

        // Determina daca butoanele Edit/Delete pot fi executate
        private bool CanExecuteEditOrDeleteAllergen(object parameter)
        {
            // Butoanele Edit/Delete sunt active doar daca:
            // 1. Un alergen este selectat in DataGrid (SelectedAllergen != null)
            // 2. NU esti deja in modul de adaugare/editare (IsEditing este false)

            bool canExecute = SelectedAllergen != null && !IsEditing;

            // Adauga aceasta linie pentru a vedea valorile in Output Window
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
                    // Verifica daca serviciul este null (cazul design-time)
                    if (_allergenService == null)
                    {
                        Debug.WriteLine("Running in design-time context, cannot delete real data.");
                        SelectedAllergen = null;
                        return;
                    }

                    // Confirma stergerea (optional)
                    // bool confirm = await _mainViewModel.ConfirmActionAsync($"Sunteti sigur ca doriti sa stergeti alergenul '{SelectedAllergen.Name}'?");
                    // if (!confirm) return;

                    await _allergenService.DeleteAllergenAsync(SelectedAllergen.Id);
                    Allergens.Remove(SelectedAllergen); // Sterge din colectia locala
                    SelectedAllergen = null; // Deselecteaza
                    SuccessMessage = "Alergenul a fost sters cu succes.";
                }
                catch (InvalidOperationException ex) // Prinde exceptia specifica din serviciu
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

            // Validare simpla - now checking AllergenName which has proper change notification
            if (string.IsNullOrWhiteSpace(AllergenName))
            {
                ErrorMessage = "Numele alergenului este obligatoriu.";
                return;
            }

            try
            {
                // Make sure CurrentAllergenForEdit.Name is updated with the latest value from AllergenName
                if (CurrentAllergenForEdit != null)
                {
                    CurrentAllergenForEdit.Name = AllergenName;
                }

                if (CurrentAllergenForEdit.Id == 0) // Adaugare
                {
                    await _allergenService.AddAllergenAsync(CurrentAllergenForEdit);
                    // Update the Id of the added object from the database
                    // This is important if you want to edit/delete it right after adding
                    // The AddAsync method usually populates the Id after SaveChangesAsync
                    Allergens.Add(CurrentAllergenForEdit); // Adauga in colectia locala
                    SuccessMessage = "Alergenul a fost adaugat cu succes.";
                }
                else // Editare
                {
                    await _allergenService.UpdateAllergenAsync(CurrentAllergenForEdit);
                    // Actualizeaza obiectul in colectia locala
                    var existingAllergen = Allergens.FirstOrDefault(a => a.Id == CurrentAllergenForEdit.Id);
                    if (existingAllergen != null)
                    {
                        existingAllergen.Name = CurrentAllergenForEdit.Name;
                    }
                    SuccessMessage = "Alergenul a fost actualizat cu succes.";
                }

                IsEditing = false; // Iese din modul editare/adaugare
                SelectedAllergen = null; // Deselecteaza
                CurrentAllergenForEdit = null; // Curata formularul
                AllergenName = string.Empty; // Clear the name field

                // Optional: Reincarca lista completa dupa salvare pentru a reflecta Id-ul pentru item-uri noi
                // await ExecuteLoadAllergens();
            }
            catch (InvalidOperationException ex) // Prinde exceptia specifica din serviciu
            {
                ErrorMessage = $"Eroare: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la salvarea alergenului: {ex.Message}";
            }
        }

        // Determina daca butonul Save poate fi executat
        private bool CanExecuteSaveAllergen(object parameter)
        {
            // Command-ul este activ doar daca suntem in modul editare/adaugare si CurrentAllergenForEdit nu este null
            // si numele nu este gol.
            bool canExecute = IsEditing && CurrentAllergenForEdit != null && !string.IsNullOrWhiteSpace(AllergenName);
            Debug.WriteLine($"CanExecuteSaveAllergen: IsEditing={IsEditing}, CurrentAllergenForEdit is null? {CurrentAllergenForEdit == null}, AllergenName='{AllergenName}', Result={canExecute}"); // Scrie in Output Window
            return canExecute;
        }

        private void ExecuteCancelEdit(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsEditing = false;
            CurrentAllergenForEdit = null;
            AllergenName = string.Empty; // Clear the name field
            SelectedAllergen = null;
            // The setters for IsEditing and CurrentAllergenForEdit will trigger CommandManager.InvalidateRequerySuggested() and explicit RaiseCanExecuteChanged
        }
    }
}
