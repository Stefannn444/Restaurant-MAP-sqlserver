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
using System.ComponentModel;

namespace RestaurantAppSQLSERVER.ViewModels
{
    public class MenuCrudViewModel : ViewModelBase
    {
        public ObservableCollection<MenuItem> MenuItems { get; set; }
        private MenuItem _selectedMenuItem;
        public MenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                _selectedMenuItem = value;
                OnPropertyChanged(nameof(SelectedMenuItem));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)EditMenuItemCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteMenuItemCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged();
            }
        }
        private MenuItem _currentMenuItemForEdit;
        public MenuItem CurrentMenuItemForEdit
        {
            get => _currentMenuItemForEdit;
            set
            {
                if (_currentMenuItemForEdit != null && SelectableDishes != null)
                {
                    foreach (var selectableDish in SelectableDishes)
                    {
                        selectableDish.PropertyChanged -= SelectableDish_PropertyChanged;
                    }
                }

                _currentMenuItemForEdit = value;
                MenuItemName = value?.Name ?? string.Empty;
                MenuItemPrice = value?.Price ?? 0m;
                MenuItemCategoryId = value?.CategoryId ?? 0;
                PhotoPath = value?.PhotoPath ?? string.Empty;
                UpdateSelectableDishes();
                if (SelectableDishes != null)
                {
                    foreach (var selectableDish in SelectableDishes)
                    {
                        selectableDish.PropertyChanged += SelectableDish_PropertyChanged;
                    }
                }


                OnPropertyChanged(nameof(CurrentMenuItemForEdit));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged();
            }
        }
        private string _menuItemName;
        public string MenuItemName
        {
            get => _menuItemName;
            set
            {
                _menuItemName = value;
                if (CurrentMenuItemForEdit != null)
                {
                    CurrentMenuItemForEdit.Name = value;
                }
                OnPropertyChanged(nameof(MenuItemName));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged();
            }
        }

        private decimal _menuItemPrice;
        public decimal MenuItemPrice
        {
            get => _menuItemPrice;
            set
            {
                _menuItemPrice = value;
                if (CurrentMenuItemForEdit != null)
                {
                    CurrentMenuItemForEdit.Price = value;
                }
                OnPropertyChanged(nameof(MenuItemPrice));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged();
            }
        }

        private int _menuItemCategoryId;
        public int MenuItemCategoryId
        {
            get => _menuItemCategoryId;
            set
            {
                _menuItemCategoryId = value;
                if (CurrentMenuItemForEdit != null)
                {
                    CurrentMenuItemForEdit.CategoryId = value;
                }
                OnPropertyChanged(nameof(MenuItemCategoryId));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged();
            }
        }

        private string _photoPath;
        public string PhotoPath
        {
            get => _photoPath;
            set
            {
                _photoPath = value;
                if (CurrentMenuItemForEdit != null)
                {
                    CurrentMenuItemForEdit.PhotoPath = value;
                }
                OnPropertyChanged(nameof(PhotoPath));
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public ObservableCollection<SelectableDishForMenu> SelectableDishes { get; set; }
        public ObservableCollection<Category> AvailableCategories { get; set; }
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)EditMenuItemCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteMenuItemCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged();
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
        public ICommand LoadMenuItemsCommand { get; }
        public ICommand AddNewMenuItemCommand { get; }
        public ICommand EditMenuItemCommand { get; }
        public ICommand DeleteMenuItemCommand { get; }
        public ICommand SaveMenuItemCommand { get; }
        public ICommand CancelEditCommand { get; }

        private readonly MenuItemService _menuItemService;
        private readonly DishService _dishService;
        private readonly CategoryService _categoryService;
        public MenuCrudViewModel() : this(null, null, null)
        {
        }
        public MenuCrudViewModel(MenuItemService menuItemService, DishService dishService, CategoryService categoryService)
        {
            _menuItemService = menuItemService ?? throw new ArgumentNullException(nameof(menuItemService));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            MenuItems = new ObservableCollection<MenuItem>();
            SelectableDishes = new ObservableCollection<SelectableDishForMenu>();
            AvailableCategories = new ObservableCollection<Category>();
            LoadMenuItemsCommand = new RelayCommand(async (param) => await ExecuteLoadMenuItems());
            AddNewMenuItemCommand = new RelayCommand(ExecuteAddNewMenuItem);
            EditMenuItemCommand = new RelayCommand(ExecuteEditMenuItem, CanExecuteEditOrDeleteMenuItem);
            DeleteMenuItemCommand = new RelayCommand(async (param) => await ExecuteDeleteMenuItem(), CanExecuteEditOrDeleteMenuItem);
            SaveMenuItemCommand = new RelayCommand(async (param) => await ExecuteSaveMenuItem(), CanExecuteSaveMenuItem);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);
            MenuItemName = string.Empty;
            MenuItemPrice = 0m;
            MenuItemCategoryId = 0;
            PhotoPath = string.Empty;
             Task.Run(async () => await ExecuteLoadMenuItems());
            Task.Run(async () => await LoadAvailableDishes());
            Task.Run(async () => await LoadAvailableCategories());
        }

        private async Task ExecuteLoadMenuItems()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                if (_menuItemService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real menus.");
                    return;
                }


                var menuItemsList = await _menuItemService.GetAllMenuItemsAsync();
                MenuItems.Clear();
                foreach (var menuItem in menuItemsList)
                {
                    MenuItems.Add(menuItem);
                }
                SuccessMessage = $"Au fost incarcate {MenuItems.Count} meniuri.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea meniurilor: {ex.Message}";
            }
        }
        private async Task LoadAvailableDishes()
        {
            if (_dishService == null)
            {
                Debug.WriteLine("Running in design-time context, cannot load real dishes.");
                return;
            }

            try
            {
                var dishes = await _dishService.GetAllDishesAsync();
                if (SelectableDishes != null)
                {
                    foreach (var selectableDish in SelectableDishes)
                    {
                        selectableDish.PropertyChanged -= SelectableDish_PropertyChanged;
                    }
                }

                SelectableDishes.Clear();
                foreach (var dish in dishes)
                {
                    var selectableDish = new SelectableDishForMenu(dish);
                    SelectableDishes.Add(selectableDish);
                    selectableDish.PropertyChanged += SelectableDish_PropertyChanged;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare la incarcarea preparatelor disponibile: {ex.Message}");
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
        private void UpdateSelectableDishes()
        {
            if (SelectableDishes == null || CurrentMenuItemForEdit == null)
            {
                return;
            }
            var currentMenuItemDishes = CurrentMenuItemForEdit.MenuItemDishes?.ToList() ?? new List<MenuItemDish>();
            foreach (var selectableDish in SelectableDishes)
            {
                var menuItemDish = currentMenuItemDishes.FirstOrDefault(mid => mid.DishId == selectableDish.Dish.Id);
                if (menuItemDish != null)
                {
                    selectableDish.IsSelected = true;
                    selectableDish.SelectedQuantity = menuItemDish.Quantity;
                }
                else
                {
                    selectableDish.IsSelected = false;
                    selectableDish.SelectedQuantity = 0;
                }
            }
        }
        private void SelectableDish_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableDishForMenu.IsSelected) || e.PropertyName == nameof(SelectableDishForMenu.SelectedQuantity))
            {
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged();
            }
        }


        private void ExecuteAddNewMenuItem(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            CurrentMenuItemForEdit = new MenuItem { MenuItemDishes = new List<MenuItemDish>() };
            IsEditing = true;
        }

        private void ExecuteEditMenuItem(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedMenuItem != null)
            {
                CurrentMenuItemForEdit = new MenuItem
                {
                    Id = SelectedMenuItem.Id,
                    Name = SelectedMenuItem.Name,
                    Price = SelectedMenuItem.Price,
                    CategoryId = SelectedMenuItem.CategoryId,
                    PhotoPath = SelectedMenuItem.PhotoPath,
                    MenuItemDishes = SelectedMenuItem.MenuItemDishes != null
                                    ? new List<MenuItemDish>(SelectedMenuItem.MenuItemDishes.Select(mid => new MenuItemDish { MenuItemId = mid.MenuItemId, DishId = mid.DishId, Quantity = mid.Quantity, Dish = mid.Dish }))
                                    : new List<MenuItemDish>()
                };
                IsEditing = true;
            }
        }
        private bool CanExecuteEditOrDeleteMenuItem(object parameter)
        {

            bool canExecute = SelectedMenuItem != null && !IsEditing;

            return canExecute;
        }

        private async Task ExecuteDeleteMenuItem()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedMenuItem != null)
            {
                try
                {
                    if (_menuItemService == null)
                    {
                        Debug.WriteLine("Running in design-time context, cannot delete real data.");
                        SelectedMenuItem = null;
                        return;
                    }

                    await _menuItemService.DeleteMenuItemAsync(SelectedMenuItem.Id);
                    MenuItems.Remove(SelectedMenuItem);
                    SelectedMenuItem = null;
                    SuccessMessage = "Meniul a fost sters cu succes.";
                }
                catch (InvalidOperationException ex)
                {
                    ErrorMessage = $"Eroare: {ex.Message}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Eroare la stergerea meniului: {ex.Message}";
                }
            }
        }

        private async Task ExecuteSaveMenuItem()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (CurrentMenuItemForEdit == null || string.IsNullOrWhiteSpace(MenuItemName) || MenuItemPrice <= 0 || MenuItemCategoryId <= 0)
            {
                ErrorMessage = "Numele, pretul si categoria meniului sunt obligatorii.";
                return;
            }
            var selectedDishesWithZeroQuantity = SelectableDishes.Where(sd => sd.IsSelected && sd.SelectedQuantity <= 0).ToList();
            if (selectedDishesWithZeroQuantity.Any())
            {
                ErrorMessage = "Preparatele selectate trebuie sa aiba o cantitate mai mare decat 0.";
                return;
            }
            if (SelectableDishes == null || !SelectableDishes.Any(sd => sd.IsSelected && sd.SelectedQuantity > 0))
            {
                ErrorMessage = "Meniul trebuie sa contina cel putin un preparat selectat cu o cantitate valida (> 0).";
                return;
            }


            try
            {
                if (_menuItemService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot save real data.");
                    IsEditing = false;
                    SelectedMenuItem = null;
                    CurrentMenuItemForEdit = null;
                    MenuItemName = string.Empty;
                    MenuItemPrice = 0m; MenuItemCategoryId = 0; PhotoPath = string.Empty;
                    foreach (var sd in SelectableDishes) { sd.IsSelected = false; sd.SelectedQuantity = 0; }
                    return;
                }
                if (CurrentMenuItemForEdit != null)
                {
                    CurrentMenuItemForEdit.Name = MenuItemName;
                    CurrentMenuItemForEdit.Price = MenuItemPrice;
                    CurrentMenuItemForEdit.CategoryId = MenuItemCategoryId;
                    CurrentMenuItemForEdit.PhotoPath = PhotoPath;
                    CurrentMenuItemForEdit.MenuItemDishes.Clear();
                    foreach (var selectableDish in SelectableDishes)
                    {
                        if (selectableDish.IsSelected && selectableDish.SelectedQuantity > 0)
                        {
                            CurrentMenuItemForEdit.MenuItemDishes.Add(new MenuItemDish
                            {
                                DishId = selectableDish.Dish.Id,
                                Quantity = selectableDish.SelectedQuantity
                            });
                        }
                    }
                }

                if (CurrentMenuItemForEdit.Id == 0)
                {
                    await _menuItemService.AddMenuItemAsync(CurrentMenuItemForEdit);
                    MenuItems.Add(CurrentMenuItemForEdit);
                    SuccessMessage = "Meniul a fost adaugat cu succes.";
                }
                else
                {
                    await _menuItemService.UpdateMenuItemAsync(CurrentMenuItemForEdit);
                    var existingMenuItem = MenuItems.FirstOrDefault(m => m.Id == CurrentMenuItemForEdit.Id);
                    if (existingMenuItem != null)
                    {
                        existingMenuItem.Name = CurrentMenuItemForEdit.Name;
                        existingMenuItem.Price = CurrentMenuItemForEdit.Price;
                        existingMenuItem.CategoryId = CurrentMenuItemForEdit.CategoryId;
                        existingMenuItem.PhotoPath = CurrentMenuItemForEdit.PhotoPath;
                    }
                    SuccessMessage = "Meniul a fost actualizat cu succes.";
                }

                IsEditing = false;
                SelectedMenuItem = null;
                CurrentMenuItemForEdit = null;
                MenuItemName = string.Empty;
                MenuItemPrice = 0m; MenuItemCategoryId = 0; PhotoPath = string.Empty;
                foreach (var sd in SelectableDishes) { sd.IsSelected = false; sd.SelectedQuantity = 0; }
                await ExecuteLoadMenuItems();
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = $"Eroare: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la salvarea meniului: {ex.Message}";
            }
        }
        private bool CanExecuteSaveMenuItem(object parameter)
        {
            return IsEditing && CurrentMenuItemForEdit != null &&
                   !string.IsNullOrWhiteSpace(MenuItemName) &&
                   MenuItemPrice > 0 &&
                   MenuItemCategoryId > 0 &&
                   SelectableDishes != null && SelectableDishes.Any(sd => sd.IsSelected && sd.SelectedQuantity > 0);
        }

        private void ExecuteCancelEdit(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsEditing = false;
            CurrentMenuItemForEdit = null;
            MenuItemName = string.Empty;
            MenuItemPrice = 0m; MenuItemCategoryId = 0; PhotoPath = string.Empty;
            foreach (var sd in SelectableDishes) { sd.IsSelected = false; sd.SelectedQuantity = 0; }
            SelectedMenuItem = null;
        }
    }
}
