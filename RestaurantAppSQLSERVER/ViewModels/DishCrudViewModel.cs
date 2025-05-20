using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.Models.Wrappers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace RestaurantAppSQLSERVER.ViewModels
{
    public class DishCrudViewModel : ViewModelBase
    {
        public ObservableCollection<Dish> Dishes { get; set; }
        private Dish _selectedDish;
        public Dish SelectedDish
        {
            get => _selectedDish;
            set
            {
                _selectedDish = value;
                OnPropertyChanged(nameof(SelectedDish));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)EditDishCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteDishCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged();
            }
        }
        private Dish _currentDishForEdit;
        public Dish CurrentDishForEdit
        {
            get => _currentDishForEdit;
            set
            {
                _currentDishForEdit = value;
                DishName = value?.Name ?? string.Empty;
                DishPrice = value?.Price ?? 0m;
                DishQuantity = value?.Quantity ?? 0;
                DishTotalQuantity = value?.TotalQuantity ?? 0;
                DishCategoryId = value?.CategoryId ?? 0;
                DishPhotoPath = value?.PhotoPath ?? string.Empty;
                DishIsAvailable = value?.IsAvailable ?? false;
                DishDescription = value?.Description ?? string.Empty;
                UpdateSelectableAllergens();

                OnPropertyChanged(nameof(CurrentDishForEdit));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged();
            }
        }
        private string _dishName;
        public string DishName
        {
            get => _dishName;
            set
            {
                _dishName = value;
                if (CurrentDishForEdit != null)
                {
                    CurrentDishForEdit.Name = value;
                }
                OnPropertyChanged(nameof(DishName));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged();
            }
        }

        private decimal _dishPrice;
        public decimal DishPrice
        {
            get => _dishPrice;
            set
            {
                _dishPrice = value;
                if (CurrentDishForEdit != null)
                {
                    CurrentDishForEdit.Price = value;
                }
                OnPropertyChanged(nameof(DishPrice));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged();
            }
        }

        private int _dishQuantity;
        public int DishQuantity
        {
            get => _dishQuantity;
            set
            {
                _dishQuantity = value;
                if (CurrentDishForEdit != null)
                {
                    CurrentDishForEdit.Quantity = value;
                }
                OnPropertyChanged(nameof(DishQuantity));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private int _dishTotalQuantity;
        public int DishTotalQuantity
        {
            get => _dishTotalQuantity;
            set
            {
                _dishTotalQuantity = value;
                if (CurrentDishForEdit != null)
                {
                    CurrentDishForEdit.TotalQuantity = value;
                }
                OnPropertyChanged(nameof(DishTotalQuantity));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private int _dishCategoryId;
        public int DishCategoryId
        {
            get => _dishCategoryId;
            set
            {
                _dishCategoryId = value;
                if (CurrentDishForEdit != null)
                {
                    CurrentDishForEdit.CategoryId = value;
                }
                OnPropertyChanged(nameof(DishCategoryId));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged();
            }
        }

        private string _dishPhotoPath;
        public string DishPhotoPath
        {
            get => _dishPhotoPath;
            set
            {
                _dishPhotoPath = value;
                if (CurrentDishForEdit != null)
                {
                    CurrentDishForEdit.PhotoPath = value;
                }
                OnPropertyChanged(nameof(DishPhotoPath));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool _dishIsAvailable;
        public bool DishIsAvailable
        {
            get => _dishIsAvailable;
            set
            {
                _dishIsAvailable = value;
                if (CurrentDishForEdit != null)
                {
                    CurrentDishForEdit.IsAvailable = value;
                }
                OnPropertyChanged(nameof(DishIsAvailable));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _dishDescription;
        public string DishDescription
        {
            get => _dishDescription;
            set
            {
                _dishDescription = value;
                if (CurrentDishForEdit != null)
                {
                    CurrentDishForEdit.Description = value;
                }
                OnPropertyChanged(nameof(DishDescription));
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public ObservableCollection<Category> AvailableCategories { get; set; }
        public ObservableCollection<SelectableAllergen> SelectableAllergens { get; set; }
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)EditDishCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteDishCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged();
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
        public ICommand LoadDishesCommand { get; }
        public ICommand AddNewDishCommand { get; }
        public ICommand EditDishCommand { get; }
        public ICommand DeleteDishCommand { get; }
        public ICommand SaveDishCommand { get; }
        public ICommand CancelEditCommand { get; }

        private readonly DishService _dishService;
        private readonly CategoryService _categoryService;
        private readonly AllergenService _allergenService;
        public DishCrudViewModel() : this(null, null, null)
        {
        }
        public DishCrudViewModel(DishService dishService, CategoryService categoryService, AllergenService allergenService)
        {
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _allergenService = allergenService ?? throw new ArgumentNullException(nameof(allergenService));
            Dishes = new ObservableCollection<Dish>();
            AvailableCategories = new ObservableCollection<Category>();
            SelectableAllergens = new ObservableCollection<SelectableAllergen>();
            LoadDishesCommand = new RelayCommand(async (param) => await ExecuteLoadDishes());
            AddNewDishCommand = new RelayCommand(ExecuteAddNewDish);
            EditDishCommand = new RelayCommand(ExecuteEditDish, CanExecuteEditOrDeleteDish);
            DeleteDishCommand = new RelayCommand(async (param) => await ExecuteDeleteDish(), CanExecuteEditOrDeleteDish);
            SaveDishCommand = new RelayCommand(async (param) => await ExecuteSaveDish(), CanExecuteSaveDish);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);
            DishName = string.Empty;
            DishPrice = 0m;
            DishQuantity = 0;
            DishTotalQuantity = 0;
            DishCategoryId = 0;
            DishPhotoPath = string.Empty;
            DishIsAvailable = false;
            DishDescription = string.Empty;
            Task.Run(async () => await ExecuteLoadDishes());
            Task.Run(async () => await LoadAvailableCategories());
            Task.Run(async () => await LoadAvailableAllergens());
        }

        private async Task ExecuteLoadDishes()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                if (_dishService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real dishes.");
                    return;
                }


                var dishesList = await _dishService.GetAllDishesAsync();
                Dishes.Clear();
                foreach (var dish in dishesList)
                {
                    Dishes.Add(dish);
                }
                SuccessMessage = $"Au fost incarcate {Dishes.Count} preparate.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea preparatelor: {ex.Message}";
            }
        }
        private async Task LoadAvailableCategories()
        {
            if (_categoryService == null)
            {
                Debug.WriteLine("Running in design-time context, cannot load real categories.");
                return;
            }

            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                AvailableCategories.Clear();
                foreach (var category in categories)
                {
                    AvailableCategories.Add(category);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare la incarcarea categoriilor disponibile: {ex.Message}");
            }
        }
        private async Task LoadAvailableAllergens()
        {
            if (_allergenService == null)
            {
                Debug.WriteLine("Running in design-time context, cannot load real allergens.");
                return;
            }

            try
            {
                var allergens = await _allergenService.GetAllAllergensAsync();
                SelectableAllergens.Clear();
                foreach (var allergen in allergens)
                {
                    SelectableAllergens.Add(new SelectableAllergen(allergen));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare la incarcarea alergenilor disponibili: {ex.Message}");
            }
        }
        private void UpdateSelectableAllergens()
        {
            if (SelectableAllergens == null || CurrentDishForEdit == null)
            {
                return;
            }
            var currentDishAllergenIds = CurrentDishForEdit.DishAllergens?.Select(da => da.AllergenId).ToList() ?? new List<int>();
            foreach (var selectableAllergen in SelectableAllergens)
            {
                selectableAllergen.IsSelected = currentDishAllergenIds.Contains(selectableAllergen.Allergen.Id);
            }
        }


        private void ExecuteAddNewDish(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            CurrentDishForEdit = new Dish { DishAllergens = new List<DishAllergen>() };
            IsEditing = true;
        }

        private void ExecuteEditDish(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedDish != null)
            {
                CurrentDishForEdit = new Dish
                {
                    Id = SelectedDish.Id,
                    Name = SelectedDish.Name,
                    Price = SelectedDish.Price,
                    Quantity = SelectedDish.Quantity,
                    TotalQuantity = SelectedDish.TotalQuantity,
                    CategoryId = SelectedDish.CategoryId,
                    PhotoPath = SelectedDish.PhotoPath,
                    IsAvailable = SelectedDish.IsAvailable,
                    Description = SelectedDish.Description,
                    DishAllergens = SelectedDish.DishAllergens != null
                                    ? new List<DishAllergen>(SelectedDish.DishAllergens.Select(da => new DishAllergen { DishId = da.DishId, AllergenId = da.AllergenId }))
                                    : new List<DishAllergen>()
                };
                IsEditing = true;
            }
        }
        private bool CanExecuteEditOrDeleteDish(object parameter)
        {

            bool canExecute = SelectedDish != null && !IsEditing;

            return canExecute;
        }

        private async Task ExecuteDeleteDish()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedDish != null)
            {
                try
                {
                    if (_dishService == null)
                    {
                        Debug.WriteLine("Running in design-time context, cannot delete real data.");
                        SelectedDish = null;
                        return;
                    }

                    await _dishService.DeleteDishAsync(SelectedDish.Id);
                    Dishes.Remove(SelectedDish);
                    SelectedDish = null;
                    SuccessMessage = "Preparatul a fost sters cu succes.";
                }
                catch (InvalidOperationException ex)
                {
                    ErrorMessage = $"Eroare: {ex.Message}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Eroare la stergerea preparatului: {ex.Message}";
                }
            }
        }

        private async Task ExecuteSaveDish()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (CurrentDishForEdit == null || string.IsNullOrWhiteSpace(DishName) || DishPrice <= 0 || DishQuantity <= 0 || DishTotalQuantity < 0 || DishCategoryId <= 0)
            {
                ErrorMessage = "Numele, pretul, cantitatea (portie), cantitatea totala si categoria sunt obligatorii.";
                return;
            }

            try
            {
                if (_dishService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot save real data.");
                    IsEditing = false;
                    SelectedDish = null;
                    CurrentDishForEdit = null;
                    DishName = string.Empty; DishPrice = 0m; DishQuantity = 0; DishTotalQuantity = 0; DishCategoryId = 0; DishPhotoPath = string.Empty; DishIsAvailable = false; DishDescription = string.Empty;
                    return;
                }
                if (CurrentDishForEdit != null)
                {
                    CurrentDishForEdit.Name = DishName;
                    CurrentDishForEdit.Price = DishPrice;
                    CurrentDishForEdit.Quantity = DishQuantity;
                    CurrentDishForEdit.TotalQuantity = DishTotalQuantity;
                    CurrentDishForEdit.CategoryId = DishCategoryId;
                    CurrentDishForEdit.PhotoPath = DishPhotoPath;
                    CurrentDishForEdit.IsAvailable = DishIsAvailable;
                    CurrentDishForEdit.Description = DishDescription;
                    CurrentDishForEdit.DishAllergens.Clear();
                    foreach (var selectableAllergen in SelectableAllergens)
                    {
                        if (selectableAllergen.IsSelected)
                        {
                            CurrentDishForEdit.DishAllergens.Add(new DishAllergen { AllergenId = selectableAllergen.Allergen.Id });
                        }
                    }
                }

                if (CurrentDishForEdit.Id == 0)
                {
                    await _dishService.AddDishAsync(CurrentDishForEdit);
                    Dishes.Add(CurrentDishForEdit);
                    SuccessMessage = "Preparatul a fost adaugat cu succes.";
                }
                else
                {
                    await _dishService.UpdateDishAsync(CurrentDishForEdit);
                    var existingDish = Dishes.FirstOrDefault(d => d.Id == CurrentDishForEdit.Id);
                    if (existingDish != null)
                    {
                        existingDish.Name = CurrentDishForEdit.Name;
                        existingDish.Price = CurrentDishForEdit.Price;
                        existingDish.Quantity = CurrentDishForEdit.Quantity;
                        existingDish.TotalQuantity = CurrentDishForEdit.TotalQuantity;
                        existingDish.CategoryId = CurrentDishForEdit.CategoryId;
                        existingDish.PhotoPath = CurrentDishForEdit.PhotoPath;
                        existingDish.IsAvailable = CurrentDishForEdit.IsAvailable;
                        existingDish.Description = CurrentDishForEdit.Description;
                    }
                    SuccessMessage = "Preparatul a fost actualizat cu succes.";
                }

                IsEditing = false;
                SelectedDish = null;
                CurrentDishForEdit = null;
                DishName = string.Empty; DishPrice = 0m; DishQuantity = 0; DishTotalQuantity = 0; DishCategoryId = 0; DishPhotoPath = string.Empty; DishIsAvailable = false; DishDescription = string.Empty;
                foreach (var sa in SelectableAllergens) sa.IsSelected = false;
                await ExecuteLoadDishes();
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = $"Eroare: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la salvarea preparatului: {ex.Message}";
            }
        }
        private bool CanExecuteSaveDish(object parameter)
        {
            return IsEditing && CurrentDishForEdit != null &&
                   !string.IsNullOrWhiteSpace(DishName) &&
                   DishPrice > 0 &&
                   DishQuantity > 0 &&
                   DishTotalQuantity >= 0 &&
                   DishCategoryId > 0;
        }

        private void ExecuteCancelEdit(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsEditing = false;
            CurrentDishForEdit = null;
            DishName = string.Empty; DishPrice = 0m; DishQuantity = 0; DishTotalQuantity = 0; DishCategoryId = 0; DishPhotoPath = string.Empty; DishIsAvailable = false; DishDescription = string.Empty;
            foreach (var sa in SelectableAllergens) sa.IsSelected = false;
            SelectedDish = null;
        }
    }
}
