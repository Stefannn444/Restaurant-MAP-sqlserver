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
    // ViewModel pentru gestionarea operatiilor CRUD pe entitatea Dish
    public class DishCrudViewModel : ViewModelBase
    {
        // Colectie pentru lista de preparate afisate
        public ObservableCollection<Dish> Dishes { get; set; }

        // Proprietate pentru selectia curenta in DataGrid
        private Dish _selectedDish;
        public Dish SelectedDish
        {
            get => _selectedDish;
            set
            {
                _selectedDish = value;
                OnPropertyChanged(nameof(SelectedDish));
                // Actualizeaza starea butoanelor Edit/Delete cand selectia se schimba
                CommandManager.InvalidateRequerySuggested(); // Notificare globala (standard)

                // Notificare EXPLICITA pentru command-urile Edit si Delete
                ((RelayCommand)EditDishCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteDishCommand).RaiseCanExecuteChanged();

                // Notificare si pentru SaveCommand (desi nu se aplica direct la selectie, e bine sa fie consistenta)
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged();
            }
        }

        // Proprietate pentru obiectul Dish folosit pentru editare/adaugare in formular
        private Dish _currentDishForEdit;
        public Dish CurrentDishForEdit
        {
            get => _currentDishForEdit;
            set
            {
                _currentDishForEdit = value;
                // Actualizeaza proprietatile individuale folosite pentru binding in formular
                DishName = value?.Name ?? string.Empty;
                DishPrice = value?.Price ?? 0m;
                DishQuantity = value?.Quantity ?? 0;
                DishTotalQuantity = value?.TotalQuantity ?? 0;
                DishCategoryId = value?.CategoryId ?? 0;
                DishPhotoPath = value?.PhotoPath ?? string.Empty;
                DishIsAvailable = value?.IsAvailable ?? false;
                DishDescription = value?.Description ?? string.Empty;

                // Actualizeaza starea de selectie a alergenilor disponibili
                UpdateSelectableAllergens();

                OnPropertyChanged(nameof(CurrentDishForEdit));
                // Notifica CommandManager cand obiectul de editare se schimba
                CommandManager.InvalidateRequerySuggested();
                // Notificare EXPLICITA pentru command-urile afectate de starea de editare
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged(); // CancelCommand state also depends on IsEditing
            }
        }

        // Proprietati individuale pentru binding in formularul de editare/adaugare
        private string _dishName;
        public string DishName
        {
            get => _dishName;
            set
            {
                _dishName = value;
                // Actualizeaza si obiectul CurrentDishForEdit
                if (CurrentDishForEdit != null)
                {
                    CurrentDishForEdit.Name = value;
                }
                OnPropertyChanged(nameof(DishName));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged(); // Notifica SaveCommand
            }
        }

        private decimal _dishPrice;
        public decimal DishPrice
        {
            get => _dishPrice;
            set
            {
                _dishPrice = value;
                // Actualizeaza si obiectul CurrentDishForEdit
                if (CurrentDishForEdit != null)
                {
                    CurrentDishForEdit.Price = value;
                }
                OnPropertyChanged(nameof(DishPrice));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged(); // Notifica SaveCommand (daca pretul afecteaza validarea)
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
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged(); // Notifica SaveCommand (daca categoria afecteaza validarea)
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

        // Colectie pentru lista de categorii disponibile pentru ComboBox
        public ObservableCollection<Category> AvailableCategories { get; set; }

        // Colectie pentru lista de alergeni disponibili cu starea de selectie pentru ListBox
        public ObservableCollection<SelectableAllergen> SelectableAllergens { get; set; }


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
                ((RelayCommand)EditDishCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteDishCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveDishCommand).RaiseCanExecuteChanged();
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
        public ICommand LoadDishesCommand { get; }
        public ICommand AddNewDishCommand { get; }
        public ICommand EditDishCommand { get; }
        public ICommand DeleteDishCommand { get; }
        public ICommand SaveDishCommand { get; }
        public ICommand CancelEditCommand { get; }

        private readonly DishService _dishService;
        private readonly CategoryService _categoryService; // Necesita CategoryService pentru a obtine lista de categorii pentru dropdown
        private readonly AllergenService _allergenService; // Necesita AllergenService pentru a obtine lista de alergeni pentru ListBox


        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        public DishCrudViewModel() : this(null, null, null)
        {
            // Poti adauga aici date mock pentru a vedea ceva in designer
            // Dishes.Add(new Dish { Id = 1, Name = "Mock Dish 1", Price = 10m, Quantity = 200, TotalQuantity = 1000, CategoryId = 1, IsAvailable = true, Category = new Category { Id = 1, Name = "Mock Categorie" } });
            // AvailableCategories = new ObservableCollection<Category> { new Category { Id = 1, Name = "Mock Categorie 1" }, new Category { Id = 2, Name = "Mock Categorie 2" } };
            // SelectableAllergens = new ObservableCollection<SelectableAllergen> { new SelectableAllergen(new Allergen { Id = 1, Name = "Gluten" }), new SelectableAllergen(new Allergen { Id = 2, Name = "Lactoză" }) };
        }


        // Constructorul principal - folosit la RULARE
        public DishCrudViewModel(DishService dishService, CategoryService categoryService, AllergenService allergenService)
        {
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService)); // Injecteaza CategoryService
            _allergenService = allergenService ?? throw new ArgumentNullException(nameof(allergenService)); // Injecteaza AllergenService

            // Initializeaza colectiile
            Dishes = new ObservableCollection<Dish>();
            AvailableCategories = new ObservableCollection<Category>();
            SelectableAllergens = new ObservableCollection<SelectableAllergen>();

            // Initializeaza command-urile
            LoadDishesCommand = new RelayCommand(async (param) => await ExecuteLoadDishes());
            AddNewDishCommand = new RelayCommand(ExecuteAddNewDish);
            EditDishCommand = new RelayCommand(ExecuteEditDish, CanExecuteEditOrDeleteDish);
            DeleteDishCommand = new RelayCommand(async (param) => await ExecuteDeleteDish(), CanExecuteEditOrDeleteDish);
            SaveDishCommand = new RelayCommand(async (param) => await ExecuteSaveDish(), CanExecuteSaveDish);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);

            // Initializeaza proprietatile pentru formular
            DishName = string.Empty;
            DishPrice = 0m;
            DishQuantity = 0;
            DishTotalQuantity = 0;
            DishCategoryId = 0;
            DishPhotoPath = string.Empty;
            DishIsAvailable = false;
            DishDescription = string.Empty;

            // Incarca datele la initializarea ViewModel-ului (optional)
            Task.Run(async () => await ExecuteLoadDishes()); // Poti incarca si lista de preparate aici
            Task.Run(async () => await LoadAvailableCategories()); // Incarca categoriile disponibile -- DECOMENATAT
            Task.Run(async () => await LoadAvailableAllergens()); // Incarca alergenii disponibili -- DECOMENATAT
        }

        // --- Metode pentru Command-uri ---

        private async Task ExecuteLoadDishes()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                // Verifica daca serviciul este null (cazul design-time)
                if (_dishService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real dishes.");
                    // Poti adauga date mock aici pentru design-time
                    // Dishes.Clear();
                    // Dishes.Add(new Dish { Id = 1, Name = "Mock Dish 1", Price = 10m, Quantity = 200, TotalQuantity = 1000, CategoryId = 1, IsAvailable = true, Category = new Category { Id = 1, Name = "Mock Categorie" } });
                    return; // Iesi din metoda daca suntem in design-time
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

        // Metoda pentru a incarca lista de alergeni disponibili pentru ListBox
        private async Task LoadAvailableAllergens()
        {
            // Verifica daca serviciul este null (cazul design-time)
            if (_allergenService == null)
            {
                Debug.WriteLine("Running in design-time context, cannot load real allergens.");
                // Adauga alergeni mock pentru design-time
                // SelectableAllergens.Clear();
                // SelectableAllergens.Add(new SelectableAllergen(new Allergen { Id = 1, Name = "Gluten" }));
                // SelectableAllergens.Add(new SelectableAllergen(new Allergen { Id = 2, Name = "Lactoză" }));
                return;
            }

            try
            {
                var allergens = await _allergenService.GetAllAllergensAsync();
                SelectableAllergens.Clear();
                foreach (var allergen in allergens)
                {
                    SelectableAllergens.Add(new SelectableAllergen(allergen)); // Creeaza wrapper pentru fiecare alergen
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare la incarcarea alergenilor disponibili: {ex.Message}");
            }
        }

        // Metoda pentru a actualiza starea de selectie a alergenilor in ListBox
        private void UpdateSelectableAllergens()
        {
            // Verifica daca avem alergeni disponibili si un preparat curent
            if (SelectableAllergens == null || CurrentDishForEdit == null)
            {
                return;
            }

            // Daca preparatul curent are legaturi DishAllergen
            var currentDishAllergenIds = CurrentDishForEdit.DishAllergens?.Select(da => da.AllergenId).ToList() ?? new List<int>();

            // Itereaza prin alergenii disponibili si seteaza IsSelected
            foreach (var selectableAllergen in SelectableAllergens)
            {
                // Seteaza IsSelected la true daca ID-ul alergenului se gaseste in lista de ID-uri ale preparatului curent
                selectableAllergen.IsSelected = currentDishAllergenIds.Contains(selectableAllergen.Allergen.Id);
            }
        }


        private void ExecuteAddNewDish(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            CurrentDishForEdit = new Dish { DishAllergens = new List<DishAllergen>() }; // Creeaza o instanta noua cu lista de alergeni initializata
            IsEditing = true; // Trece in modul editare/adaugare
                              // Proprietatile individuale sunt actualizate prin setter-ul CurrentDishForEdit
        }

        private void ExecuteEditDish(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedDish != null)
            {
                // Creeaza o copie a obiectului selectat pentru editare
                // Asigura-te ca relatia DishAllergens este copiata (ar trebui sa fie incarcata de GetAllDishesAsync)
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
                    // Copiaza legaturile DishAllergen - IMPORTANT pentru salvare
                    // Creeaza o noua lista pentru a evita modificarea directa a obiectului din colectia principala
                    DishAllergens = SelectedDish.DishAllergens != null
                                    ? new List<DishAllergen>(SelectedDish.DishAllergens.Select(da => new DishAllergen { DishId = da.DishId, AllergenId = da.AllergenId }))
                                    : new List<DishAllergen>()
                };
                IsEditing = true; // Trece in modul editare
                                  // Proprietatile individuale sunt actualizate prin setter-ul CurrentDishForEdit
            }
        }

        // Determina daca butoanele Edit/Delete pot fi executate
        private bool CanExecuteEditOrDeleteDish(object parameter)
        {
            // Butoanele Edit/Delete sunt active doar daca:
            // 1. Un preparat este selectat in DataGrid (SelectedDish != null)
            // 2. NU esti deja in modul de adaugare/editare (IsEditing este false)

            bool canExecute = SelectedDish != null && !IsEditing;

            // Adauga aceasta linie pentru a vedea valorile in Output Window (optional)
            // Debug.WriteLine($"CanExecuteEditOrDeleteDish: SelectedDish is null? {SelectedDish == null}, IsEditing={IsEditing}, Result={canExecute}");

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
                    // Verifica daca serviciul este null (cazul design-time)
                    if (_dishService == null)
                    {
                        Debug.WriteLine("Running in design-time context, cannot delete real data.");
                        // Simuleaza stergerea din colectia mock
                        // Dishes.Remove(SelectedDish);
                        SelectedDish = null;
                        return;
                    }


                    // Confirma stergerea (optional)
                    // bool confirm = await _mainViewModel.ConfirmActionAsync($"Sunteti sigur ca doriti sa stergeti preparatul '{SelectedDish.Name}'?");
                    // if (!confirm) return;

                    await _dishService.DeleteDishAsync(SelectedDish.Id);
                    Dishes.Remove(SelectedDish); // Sterge din colectia locala
                    SelectedDish = null; // Deselecteaza
                    SuccessMessage = "Preparatul a fost sters cu succes.";
                }
                catch (InvalidOperationException ex) // Prinde exceptia specifica din serviciu
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

            // Validare simpla
            if (CurrentDishForEdit == null || string.IsNullOrWhiteSpace(DishName) || DishPrice <= 0 || DishQuantity <= 0 || DishTotalQuantity < 0 || DishCategoryId <= 0)
            {
                ErrorMessage = "Numele, pretul, cantitatea (portie), cantitatea totala si categoria sunt obligatorii.";
                return;
            }

            try
            {
                // Verifica daca serviciul este null (cazul design-time)
                if (_dishService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot save real data.");
                    // Simuleaza salvarea in colectia mock
                    // if (CurrentDishForEdit.Id == 0) { CurrentDishForEdit.Id = Dishes.Count + 1; Dishes.Add(CurrentDishForEdit); }
                    // else { var existing = Dishes.FirstOrDefault(d => d.Id == CurrentDishForEdit.Id); if (existing != null) { /* update properties */ } }
                    IsEditing = false;
                    SelectedDish = null;
                    CurrentDishForEdit = null;
                    // Curata campurile individuale
                    DishName = string.Empty; DishPrice = 0m; DishQuantity = 0; DishTotalQuantity = 0; DishCategoryId = 0; DishPhotoPath = string.Empty; DishIsAvailable = false; DishDescription = string.Empty;
                    // Clear selectable allergens state
                    // foreach(var sa in SelectableAllergens) sa.IsSelected = false;
                    return;
                }

                // Make sure CurrentDishForEdit properties are updated from individual properties
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

                    // --- Gestionarea relatiei Many-to-Many cu Alergeni (DishAllergen) ---
                    // Construieste colectia DishAllergens pe baza alergenilor selectati in UI
                    CurrentDishForEdit.DishAllergens.Clear(); // Curata legaturile existente pe obiectul din ViewModel
                    foreach (var selectableAllergen in SelectableAllergens)
                    {
                        if (selectableAllergen.IsSelected)
                        {
                            // Adauga o noua legatura DishAllergen pentru fiecare alergen selectat
                            // DishId va fi setat de EF Core la salvare pentru item-uri noi
                            CurrentDishForEdit.DishAllergens.Add(new DishAllergen { AllergenId = selectableAllergen.Allergen.Id });
                        }
                    }
                    // --- Sfarsit gestionare Alergeni ---
                }

                if (CurrentDishForEdit.Id == 0) // Adaugare
                {
                    await _dishService.AddDishAsync(CurrentDishForEdit);
                    // Update the Id of the added object from the database
                    Dishes.Add(CurrentDishForEdit); // Adauga in colectia locala
                    SuccessMessage = "Preparatul a fost adaugat cu succes.";
                }
                else // Editare
                {
                    await _dishService.UpdateDishAsync(CurrentDishForEdit);
                    // Actualizeaza obiectul in colectia locala
                    var existingDish = Dishes.FirstOrDefault(d => d.Id == CurrentDishForEdit.Id);
                    if (existingDish != null)
                    {
                        // Copiem proprietatile actualizate
                        existingDish.Name = CurrentDishForEdit.Name;
                        existingDish.Price = CurrentDishForEdit.Price;
                        existingDish.Quantity = CurrentDishForEdit.Quantity;
                        existingDish.TotalQuantity = CurrentDishForEdit.TotalQuantity;
                        existingDish.CategoryId = CurrentDishForEdit.CategoryId;
                        existingDish.PhotoPath = CurrentDishForEdit.PhotoPath;
                        existingDish.IsAvailable = CurrentDishForEdit.IsAvailable;
                        existingDish.Description = CurrentDishForEdit.Description;

                        // TODO: Actualizeaza si colectia DishAllergens pe obiectul existent in colectie
                        // Aceasta este necesara pentru ca UI-ul sa se actualizeze corect in DataGrid
                        // daca afisezi alergenii acolo. O abordare simpla este sa reincarci lista completa.
                        // existingDish.DishAllergens = new List<DishAllergen>(CurrentDishForEdit.DishAllergens);
                    }
                    SuccessMessage = "Preparatul a fost actualizat cu succes.";
                }

                IsEditing = false; // Iese din modul editare/adaugare
                SelectedDish = null; // Deselecteaza
                CurrentDishForEdit = null; // Curata formularul
                // Curata campurile individuale
                DishName = string.Empty; DishPrice = 0m; DishQuantity = 0; DishTotalQuantity = 0; DishCategoryId = 0; DishPhotoPath = string.Empty; DishIsAvailable = false; DishDescription = string.Empty;
                // Clear selectable allergens state
                foreach (var sa in SelectableAllergens) sa.IsSelected = false;


                // Optional: Reincarca lista completa dupa salvare pentru a reflecta Id-ul pentru item-uri noi si relatiile cu alergenii
                await ExecuteLoadDishes();
            }
            catch (InvalidOperationException ex) // Prinde exceptia specifica din serviciu
            {
                ErrorMessage = $"Eroare: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la salvarea preparatului: {ex.Message}";
            }
        }

        // Determina daca butonul Save poate fi executat
        private bool CanExecuteSaveDish(object parameter)
        {
            // Command-ul este activ doar daca suntem in modul editare/adaugare si CurrentDishForEdit nu este null
            // si numele, pretul, cantitatea (portie), cantitatea totala si categoria sunt valide.
            return IsEditing && CurrentDishForEdit != null &&
                   !string.IsNullOrWhiteSpace(DishName) &&
                   DishPrice > 0 &&
                   DishQuantity > 0 &&
                   DishTotalQuantity >= 0 &&
                   DishCategoryId > 0; // Categoria trebuie sa fie selectata/setata
        }

        private void ExecuteCancelEdit(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsEditing = false;
            CurrentDishForEdit = null;
            // Curata campurile individuale
            DishName = string.Empty; DishPrice = 0m; DishQuantity = 0; DishTotalQuantity = 0; DishCategoryId = 0; DishPhotoPath = string.Empty; DishIsAvailable = false; DishDescription = string.Empty;
            // Clear selectable allergens state
            foreach (var sa in SelectableAllergens) sa.IsSelected = false;
            SelectedDish = null;
            // The setters for IsEditing and CurrentDishForEdit will trigger CommandManager.InvalidateRequerySuggested() and explicit RaiseCanExecuteChanged
        }
    }
}
