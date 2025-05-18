using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.Models.Wrappers; // Using the wrapper class
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input; // For ICommand and CommandManager
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace RestaurantAppSQLSERVER.ViewModels
{
    // ViewModel pentru gestionarea operatiilor CRUD pe entitatea MenuItem (Meniul principal)
    public class MenuCrudViewModel : ViewModelBase
    {
        // Colectie pentru lista de Meniuri (MenuItem) afisate
        public ObservableCollection<MenuItem> MenuItems { get; set; } // Renamed from Menus to MenuItem for clarity based on entity

        // Proprietate pentru selectia curenta in DataGrid
        private MenuItem _selectedMenuItem; // Renamed from SelectedMenu
        public MenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                _selectedMenuItem = value;
                OnPropertyChanged(nameof(SelectedMenuItem));
                // Actualizeaza starea butoanelor Edit/Delete cand selectia se schimba
                CommandManager.InvalidateRequerySuggested(); // Notificare globala (standard)

                // Notificare EXPLICITA pentru command-urile Edit si Delete
                ((RelayCommand)EditMenuItemCommand).RaiseCanExecuteChanged(); // Renamed command
                ((RelayCommand)DeleteMenuItemCommand).RaiseCanExecuteChanged(); // Renamed command

                // Notificare si pentru SaveCommand (desi nu se aplica direct la selectie, e bine sa fie consistenta)
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged(); // Renamed command
            }
        }

        // Proprietate pentru obiectul MenuItem (Meniu) folosit pentru editare/adaugare in formular
        private MenuItem _currentMenuItemForEdit; // Renamed from CurrentMenuForEdit
        public MenuItem CurrentMenuItemForEdit
        {
            get => _currentMenuItemForEdit;
            set
            {
                _currentMenuItemForEdit = value;
                // Actualizeaza proprietatile individuale folosite pentru binding in formular
                MenuItemName = value?.Name ?? string.Empty;
                // MenuItemDescription = value?.Description ?? string.Empty; // Eliminat Description
                MenuItemPrice = value?.Price ?? 0m;
                MenuItemCategoryId = value?.CategoryId ?? 0; // Include CategoryId
                PhotoPath = value?.PhotoPath ?? string.Empty; // Folosim numele corect PhotoPath

                // Actualizeaza starea de selectie a preparatelor disponibile
                UpdateSelectableDishes();

                OnPropertyChanged(nameof(CurrentMenuItemForEdit));
                // Notifica CommandManager cand obiectul de editare se schimba
                CommandManager.InvalidateRequerySuggested();
                // Notificare EXPLICITA pentru command-urile afectate de starea de editare
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged(); // CancelCommand state also depends on IsEditing
            }
        }

        // Proprietati individuale pentru binding in formularul de editare/adaugare
        private string _menuItemName; // Renamed
        public string MenuItemName
        {
            get => _menuItemName;
            set
            {
                _menuItemName = value;
                // Actualizeaza si obiectul CurrentMenuItemForEdit
                if (CurrentMenuItemForEdit != null)
                {
                    CurrentMenuItemForEdit.Name = value;
                }
                OnPropertyChanged(nameof(MenuItemName));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged(); // Notifica SaveCommand
            }
        }

        // Eliminat Proprietatea MenuItemDescription

        private decimal _menuItemPrice; // Renamed
        public decimal MenuItemPrice
        {
            get => _menuItemPrice;
            set
            {
                _menuItemPrice = value;
                // Actualizeaza si obiectul CurrentMenuItemForEdit
                if (CurrentMenuItemForEdit != null)
                {
                    CurrentMenuItemForEdit.Price = value;
                }
                OnPropertyChanged(nameof(MenuItemPrice));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged(); // Notifica SaveCommand (daca pretul afecteaza validarea)
            }
        }

        private int _menuItemCategoryId; // Include CategoryId
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
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged(); // Notifica SaveCommand (daca categoria afecteaza validarea)
            }
        }

        private string _photoPath; // Folosim numele corect PhotoPath
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
                OnPropertyChanged(nameof(PhotoPath)); // Folosim nameof(PhotoPath)
                CommandManager.InvalidateRequerySuggested();
            }
        }


        // Colectie pentru lista de preparate disponibile cu starea de selectie pentru ListBox
        public ObservableCollection<SelectableDishForMenu> SelectableDishes { get; set; }

        // Colectie pentru lista de categorii disponibile pentru ComboBox
        public ObservableCollection<Category> AvailableCategories { get; set; }


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
                ((RelayCommand)EditMenuItemCommand).RaiseCanExecuteChanged(); // Renamed command
                ((RelayCommand)DeleteMenuItemCommand).RaiseCanExecuteChanged(); // Renamed command
                ((RelayCommand)SaveMenuItemCommand).RaiseCanExecuteChanged(); // Renamed command
                ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged(); // CancelCommand state also depends on IsEditing
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
        public ICommand LoadMenuItemsCommand { get; } // Renamed
        public ICommand AddNewMenuItemCommand { get; } // Renamed
        public ICommand EditMenuItemCommand { get; } // Renamed
        public ICommand DeleteMenuItemCommand { get; } // Renamed
        public ICommand SaveMenuItemCommand { get; } // Renamed
        public ICommand CancelEditCommand { get; }

        private readonly MenuItemService _menuItemService; // Serviciul principal pentru MenuItem (Meniu)
        private readonly DishService _dishService; // Necesita DishService pentru a obtine lista de preparate disponibile
        private readonly CategoryService _categoryService; // Necesita CategoryService pentru a obtine lista de categorii pentru dropdown


        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        public MenuCrudViewModel() : this(null, null, null)
        {
            // Poti adauga aici date mock pentru a vedea ceva in designer
            // MenuItems.Add(new MenuItem { Id = 1, Name = "Mock Meniu 1", Price = 25m, Category = new Category { Name = "Mock Categorie" } });
            // SelectableDishes = new ObservableCollection<SelectableDishForMenu> { new SelectableDishForMenu(new Dish { Id = 1, Name = "Mock Preparat 1" }), new SelectableDishForMenu(new Dish { Id = 2, Name = "Mock Preparat 2" }) };
            // AvailableCategories = new ObservableCollection<Category> { new Category { Id = 1, Name = "Mock Categorie 1" }, new Category { Id = 2, Name = "Mock Categorie 2" } };
        }


        // Constructorul principal - folosit la RULARE
        public MenuCrudViewModel(MenuItemService menuItemService, DishService dishService, CategoryService categoryService) // Injecteaza MenuItemService si DishService, CategoryService
        {
            _menuItemService = menuItemService ?? throw new ArgumentNullException(nameof(menuItemService)); // Injecteaza MenuItemService
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService)); // Injecteaza DishService
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService)); // Injecteaza CategoryService

            // Initializeaza colectiile
            MenuItems = new ObservableCollection<MenuItem>(); // Colectia de Meniuri
            SelectableDishes = new ObservableCollection<SelectableDishForMenu>(); // Colectia de preparate disponibile pentru selectie
            AvailableCategories = new ObservableCollection<Category>(); // Colectia de categorii disponibile

            // Initializeaza command-urile
            LoadMenuItemsCommand = new RelayCommand(async (param) => await ExecuteLoadMenuItems()); // Renamed
            AddNewMenuItemCommand = new RelayCommand(ExecuteAddNewMenuItem); // Renamed
            EditMenuItemCommand = new RelayCommand(ExecuteEditMenuItem, CanExecuteEditOrDeleteMenuItem); // Renamed
            DeleteMenuItemCommand = new RelayCommand(async (param) => await ExecuteDeleteMenuItem(), CanExecuteEditOrDeleteMenuItem); // Renamed
            SaveMenuItemCommand = new RelayCommand(async (param) => await ExecuteSaveMenuItem(), CanExecuteSaveMenuItem); // Renamed
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);

            // Initializeaza proprietatile pentru formular
            MenuItemName = string.Empty;
            // MenuItemDescription = string.Empty; // Eliminat Description
            MenuItemPrice = 0m;
            MenuItemCategoryId = 0;
            PhotoPath = string.Empty; // Initializeaza PhotoPath


            // Incarca datele la initializarea ViewModel-ului
            // Task.Run(async () => await ExecuteLoadMenuItems()); // Poti incarca si lista de meniuri aici
            Task.Run(async () => await LoadAvailableDishes()); // Incarca preparatele disponibile
            Task.Run(async () => await LoadAvailableCategories()); // Incarca categoriile disponibile
        }

        // --- Metode pentru Command-uri ---

        private async Task ExecuteLoadMenuItems() // Renamed
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                // Verifica daca serviciul este null (cazul design-time)
                if (_menuItemService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real menus.");
                    // Poti adauga date mock aici pentru design-time
                    // MenuItems.Clear();
                    // MenuItems.Add(new MenuItem { Id = 1, Name = "Mock Meniu 1", Price = 25m, Category = new Category { Name = "Mock Categorie" }, MenuItemDishes = new List<MenuItemDish> { new MenuItemDish { Dish = new Dish { Name = "Mock Preparat 1" } } } });
                    return; // Iesi din metoda daca suntem in design-time
                }


                var menuItemsList = await _menuItemService.GetAllMenuItemsAsync(); // Apeleaza serviciul principal
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

        // Metoda pentru a incarca lista de preparate disponibile pentru ListBox
        private async Task LoadAvailableDishes()
        {
            // Verifica daca serviciul este null (cazul design-time)
            if (_dishService == null)
            {
                Debug.WriteLine("Running in design-time context, cannot load real dishes.");
                // Adauga preparate mock pentru design-time
                // SelectableDishes.Clear();
                // SelectableDishes.Add(new SelectableDishForMenu(new Dish { Id = 1, Name = "Mock Preparat 1" }));
                // SelectableDishes.Add(new SelectableDishForMenu(new Dish { Id = 2, Name = "Mock Preparat 2" }));
                return;
            }

            try
            {
                var dishes = await _dishService.GetAllDishesAsync();
                SelectableDishes.Clear();
                foreach (var dish in dishes)
                {
                    SelectableDishes.Add(new SelectableDishForMenu(dish)); // Creeaza wrapper pentru fiecare preparat
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare la incarcarea preparatelor disponibile: {ex.Message}");
            }
        }

        // Metoda pentru a incarca lista de categorii disponibile pentru ComboBox
        private async Task LoadAvailableCategories()
        {
            // Verifica daca serviciul este null (cazul design-time)
            if (_categoryService == null)
            {
                Debug.WriteLine("Running in design-time context, cannot load real categories.");
                // Adauga categorii mock pentru design-time
                // AvailableCategories.Clear();
                // AvailableCategories.Add(new Category { Id = 1, Name = "Mock Categorie 1" });
                // AvailableCategories.Add(new Category { Id = 2, Name = "Mock Categorie 2" });
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


        // Metoda pentru a actualiza starea de selectie a preparatelor in ListBox
        private void UpdateSelectableDishes()
        {
            // Verifica daca avem preparate disponibile si un meniu curent
            if (SelectableDishes == null || CurrentMenuItemForEdit == null) // Folosim CurrentMenuItemForEdit
            {
                return;
            }

            // Daca meniul curent are MenuItemDish-uri
            var currentMenuItemDishDishIds = CurrentMenuItemForEdit.MenuItemDishes?.Select(mid => mid.DishId).ToList() ?? new List<int>(); // Folosim MenuItemDishes si DishId

            // Itereaza prin preparatele disponibile si seteaza IsSelected
            foreach (var selectableDish in SelectableDishes)
            {
                // Seteaza IsSelected la true daca ID-ul preparatului se gaseste in lista de ID-uri ale MenuItemDish-urilor din meniul curent
                selectableDish.IsSelected = currentMenuItemDishDishIds.Contains(selectableDish.Dish.Id);
            }
        }


        private void ExecuteAddNewMenuItem(object parameter) // Renamed
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            CurrentMenuItemForEdit = new MenuItem { MenuItemDishes = new List<MenuItemDish>() }; // Creeaza o instanta noua cu lista de MenuItemDish initializata
            IsEditing = true; // Trece in modul editare/adaugare
                              // Proprietatile individuale sunt actualizate prin setter-ul CurrentMenuItemForEdit
        }

        private void ExecuteEditMenuItem(object parameter) // Renamed
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedMenuItem != null) // Folosim SelectedMenuItem
            {
                // Creeaza o copie a obiectului selectat pentru editare
                // Asigura-te ca relatia MenuItemDishes este copiata (ar trebui sa fie incarcata de GetAllMenuItemsAsync)
                CurrentMenuItemForEdit = new MenuItem
                {
                    Id = SelectedMenuItem.Id,
                    Name = SelectedMenuItem.Name,
                    // Description = SelectedMenuItem.Description, // Eliminat Description
                    Price = SelectedMenuItem.Price,
                    CategoryId = SelectedMenuItem.CategoryId, // Include CategoryId
                    PhotoPath = SelectedMenuItem.PhotoPath, // Include PhotoPath
                    // Copiaza legaturile MenuItemDish - IMPORTANT pentru salvare
                    // Creeaza o noua lista pentru a evita modificarea directa a obiectului din colectia principala
                    MenuItemDishes = SelectedMenuItem.MenuItemDishes != null
                                    ? new List<MenuItemDish>(SelectedMenuItem.MenuItemDishes.Select(mid => new MenuItemDish { MenuItemId = mid.MenuItemId, DishId = mid.DishId, Quantity = mid.Quantity, Dish = mid.Dish })) // Include si Quantity si Dish
                                    : new List<MenuItemDish>()
                };
                IsEditing = true; // Trece in modul editare
                                  // Proprietatile individuale sunt actualizate prin setter-ul CurrentMenuItemForEdit
            }
        }

        // Determina daca butoanele Edit/Delete pot fi executate
        private bool CanExecuteEditOrDeleteMenuItem(object parameter) // Renamed
        {
            // Butoanele Edit/Delete sunt active doar daca:
            // 1. Un meniu este selectat in DataGrid (SelectedMenuItem != null)
            // 2. NU esti deja in modul de adaugare/editare (IsEditing este false)

            bool canExecute = SelectedMenuItem != null && !IsEditing; // Folosim SelectedMenuItem

            // Adauga aceasta linie pentru a vedea valorile in Output Window (optional)
            // Debug.WriteLine($"CanExecuteEditOrDeleteMenuItem: SelectedMenuItem is null? {SelectedMenuItem == null}, IsEditing={IsEditing}, Result={canExecute}");

            return canExecute;
        }

        private async Task ExecuteDeleteMenuItem() // Renamed
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedMenuItem != null) // Folosim SelectedMenuItem
            {
                try
                {
                    // Verifica daca serviciul este null (cazul design-time)
                    if (_menuItemService == null)
                    {
                        Debug.WriteLine("Running in design-time context, cannot delete real data.");
                        // Simuleaza stergerea din colectia mock
                        // MenuItems.Remove(SelectedMenuItem);
                        SelectedMenuItem = null;
                        return;
                    }


                    // Confirma stergerea (optional)
                    // bool confirm = await _mainViewModel.ConfirmActionAsync($"Sunteti sigur ca doriti sa stergeti meniul '{SelectedMenuItem.Name}'?");
                    // if (!confirm) return;

                    await _menuItemService.DeleteMenuItemAsync(SelectedMenuItem.Id); // Apeleaza serviciul principal
                    MenuItems.Remove(SelectedMenuItem); // Sterge din colectia locala
                    SelectedMenuItem = null; // Deselecteaza
                    SuccessMessage = "Meniul a fost sters cu succes.";
                }
                catch (InvalidOperationException ex) // Prinde exceptia specifica din serviciu
                {
                    ErrorMessage = $"Eroare: {ex.Message}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Eroare la stergerea meniului: {ex.Message}";
                }
            }
        }

        private async Task ExecuteSaveMenuItem() // Renamed
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            // Validare simpla
            if (CurrentMenuItemForEdit == null || string.IsNullOrWhiteSpace(MenuItemName) || MenuItemPrice <= 0 || MenuItemCategoryId <= 0) // Validare pentru MenuItem
            {
                ErrorMessage = "Numele, pretul si categoria meniului sunt obligatorii.";
                return;
            }

            try
            {
                // Verifica daca serviciul este null (cazul design-time)
                if (_menuItemService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot save real data.");
                    // Simuleaza salvarea in colectia mock
                    // if (CurrentMenuItemForEdit.Id == 0) { CurrentMenuItemForEdit.Id = MenuItems.Count + 1; MenuItems.Add(CurrentMenuItemForEdit); }
                    // else { var existing = MenuItems.FirstOrDefault(m => m.Id == CurrentMenuItemForEdit.Id); if (existing != null) { /* update properties */ } }
                    IsEditing = false;
                    SelectedMenuItem = null;
                    CurrentMenuItemForEdit = null;
                    // Curata campurile individuale
                    MenuItemName = string.Empty; // MenuItemDescription = string.Empty; // Eliminat Description
                    MenuItemPrice = 0m; MenuItemCategoryId = 0; PhotoPath = string.Empty; // Curata PhotoPath
                    // Clear selectable dishes state
                    // foreach(var sd in SelectableDishes) sd.IsSelected = false;
                    return;
                }

                // Make sure CurrentMenuItemForEdit properties are updated from individual properties
                if (CurrentMenuItemForEdit != null)
                {
                    CurrentMenuItemForEdit.Name = MenuItemName;
                    // CurrentMenuItemForEdit.Description = MenuItemDescription; // Eliminat Description
                    CurrentMenuItemForEdit.Price = MenuItemPrice;
                    CurrentMenuItemForEdit.CategoryId = MenuItemCategoryId;
                    CurrentMenuItemForEdit.PhotoPath = PhotoPath; // Actualizeaza PhotoPath

                    // --- Gestionarea relatiei Many-to-Many cu Preparate (prin MenuItemDish) ---
                    // Construieste colectia MenuItemDishes pe baza preparatelor selectate in UI
                    CurrentMenuItemForEdit.MenuItemDishes.Clear(); // Curata legaturile existente pe obiectul din ViewModel
                    foreach (var selectableDish in SelectableDishes)
                    {
                        if (selectableDish.IsSelected)
                        {
                            // Adauga un nou MenuItemDish pentru fiecare preparat selectat
                            // MenuItemId va fi setat de EF Core la salvare pentru item-uri noi
                            // Quantity ar trebui sa fie gestionat aici daca este relevant la adaugare
                            CurrentMenuItemForEdit.MenuItemDishes.Add(new MenuItemDish { DishId = selectableDish.Dish.Id, Quantity = 1 }); // Seteaza Quantity implicit 1 sau 0
                        }
                    }
                    // --- Sfarsit gestionare MenuItemDish ---
                }

                if (CurrentMenuItemForEdit.Id == 0) // Adaugare
                {
                    await _menuItemService.AddMenuItemAsync(CurrentMenuItemForEdit); // Apeleaza serviciul principal
                    // Update the Id of the added object from the database
                    MenuItems.Add(CurrentMenuItemForEdit); // Adauga in colectia locala
                    SuccessMessage = "Meniul a fost adaugat cu succes.";
                }
                else // Editare
                {
                    await _menuItemService.UpdateMenuItemAsync(CurrentMenuItemForEdit); // Apeleaza serviciul principal
                    // Actualizeaza obiectul in colectia locala
                    var existingMenuItem = MenuItems.FirstOrDefault(m => m.Id == CurrentMenuItemForEdit.Id);
                    if (existingMenuItem != null)
                    {
                        // Copiem proprietatile actualizate
                        existingMenuItem.Name = CurrentMenuItemForEdit.Name;
                        // existingMenuItem.Description = CurrentMenuItemForEdit.Description; // Eliminat Description
                        existingMenuItem.Price = CurrentMenuItemForEdit.Price;
                        existingMenuItem.CategoryId = CurrentMenuItemForEdit.CategoryId;
                        existingMenuItem.PhotoPath = CurrentMenuItemForEdit.PhotoPath; // Actualizeaza PhotoPath

                        // TODO: Actualizeaza si colectia MenuItemDishes pe obiectul existent in colectie
                        // Aceasta este necesara pentru ca UI-ul sa se actualizeze corect in DataGrid
                        // daca afisezi preparatele asociate acolo. O abordare simpla este sa reincarci lista completa.
                        // existingMenuItem.MenuItemDishes = new List<MenuItemDish>(CurrentMenuItemForEdit.MenuItemDishes);
                    }
                    SuccessMessage = "Meniul a fost actualizat cu succes.";
                }

                IsEditing = false; // Iese din modul editare/adaugare
                SelectedMenuItem = null; // Deselecteaza
                CurrentMenuItemForEdit = null; // Curata formularul
                // Curata campurile individuale
                MenuItemName = string.Empty; // MenuItemDescription = string.Empty; // Eliminat Description
                MenuItemPrice = 0m; MenuItemCategoryId = 0; PhotoPath = string.Empty; // Curata PhotoPath
                // Clear selectable dishes state
                foreach (var sd in SelectableDishes) sd.IsSelected = false;


                // Optional: Reincarca lista completa dupa salvare pentru a reflecta Id-ul pentru item-uri noi si relatiile cu MenuItemDish-urile
                await ExecuteLoadMenuItems(); // Reincarca lista de Meniuri
            }
            catch (InvalidOperationException ex) // Prinde exceptia specifica din serviciu
            {
                ErrorMessage = $"Eroare: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la salvarea meniului: {ex.Message}";
            }
        }

        // Determina daca butonul Save poate fi executat
        private bool CanExecuteSaveMenuItem(object parameter) // Renamed
        {
            // Command-ul este activ doar daca suntem in modul editare/adaugare si CurrentMenuItemForEdit nu este null
            // si numele, pretul si categoria sunt valide.
            return IsEditing && CurrentMenuItemForEdit != null &&
                   !string.IsNullOrWhiteSpace(MenuItemName) &&
                   MenuItemPrice > 0 &&
                   MenuItemCategoryId > 0; // Categoria trebuie sa fie selectata/setata
        }

        private void ExecuteCancelEdit(object parameter) // Renamed
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsEditing = false;
            CurrentMenuItemForEdit = null;
            // Curata campurile individuale
            MenuItemName = string.Empty; // MenuItemDescription = string.Empty; // Eliminat Description
            MenuItemPrice = 0m; MenuItemCategoryId = 0; PhotoPath = string.Empty; // Curata PhotoPath
            // Clear selectable dishes state
            foreach (var sd in SelectableDishes) sd.IsSelected = false;
            SelectedMenuItem = null;
            // The setters for IsEditing and CurrentMenuItemForEdit will trigger CommandManager.InvalidateRequerySuggested() and explicit RaiseCanExecuteChanged
        }
    }
}
