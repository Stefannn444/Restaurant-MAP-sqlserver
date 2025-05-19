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
using System.ComponentModel; // Adaugat pentru IPropertyChangedEvent

namespace RestaurantAppSQLSERVER.ViewModels
{
    // ViewModel principal pentru Dashboard-ul Clientului (si Invitatului)
    public class ClientDashboardViewModel : ViewModelBase
    {
        // Colectie pentru afisarea meniului grupat pe categorii
        public ObservableCollection<CategoryDisplayWrapper> MenuCategories { get; set; }

        // Proprietate pentru a indica daca sesiunea este de invitat
        private bool _isGuest;
        public bool IsGuest
        {
            get => _isGuest;
            set
            {
                _isGuest = value;
                OnPropertyChanged(nameof(IsGuest));
                // Notifica UI-ul ca starea de invitat s-a schimbat, afectand vizibilitatea/starea unor controale
                CommandManager.InvalidateRequerySuggested();
                // Notificare explicita pentru command-uri care depind de IsGuest (ex: ShowClientOrdersCommand, AddToCartCommand)
                // Verifica daca ShowClientOrdersCommand NU este null inainte de a apela RaiseCanExecuteChanged
                ((RelayCommand)ShowClientOrdersCommand)?.RaiseCanExecuteChanged();
                // TODO: Adauga RaiseCanExecuteChanged() pentru AddToCartCommand cand il implementezi
                // ((RelayCommand)AddToCartCommand)?.RaiseCanExecuteChanged();
            }
        }


        // Proprietate pentru a afisa informatii despre utilizatorul autentificat (va fi null pentru invitati)
        private User _loggedInUser;
        public User LoggedInUser
        {
            get => _loggedInUser;
            set
            {
                _loggedInUser = value;
                OnPropertyChanged(nameof(LoggedInUser));
                // Actualizeaza si starea IsGuest pe baza LoggedInUser
                // Acest lucru va apela setter-ul IsGuest, care acum verifica daca command-urile sunt initializate
                IsGuest = (value == null);
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

        // Command-uri pentru navigare (ex: catre sectiunea Comenzi Client)
        public ICommand ShowClientOrdersCommand { get; }
        public ICommand LogoutCommand { get; } // Command pentru Logout
        // Noul Command pentru a naviga inapoi la Login (pentru invitati)
        public ICommand ShowLoginCommand { get; }

        // TODO: Adauga ICommand pentru adaugare in cos (AddToCartCommand)


        private readonly CategoryService _categoryService;
        private readonly DishService _dishService;
        private readonly MenuItemService _menuItemService; // Serviciul principal pentru MenuItem (Meniu)
        private readonly MainViewModel _mainViewModel; // Referenta catre MainViewModel pentru navigare/logout


        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        public ClientDashboardViewModel() : this(null, null, null, null, null)
        {
            Debug.WriteLine("ClientDashboardViewModel created for Design Time.");
            // Poti adauga aici date mock pentru a vedea ceva in designer
            // LoggedInUser = new User { Nume = "Client Mock" }; // Foloseste Nume conform entitatii User
            // IsGuest = false; // Seteaza starea pentru design-time daca vrei sa vezi UI-ul de client
            // Initializeaza colectia pentru design time
            // MenuCategories = new ObservableCollection<CategoryDisplayWrapper>();
            // MenuCategories.Add(new CategoryDisplayWrapper(new Category { Name = "Mock Categorie 1" }));
            // MenuCategories[0].DisplayItems.Add(new DisplayMenuItem(new Dish { Name = "Mock Dish 1", Price = 10m }));
        }


        // Constructorul principal - folosit la RULARE
        public ClientDashboardViewModel(User loggedInUser, CategoryService categoryService, DishService dishService, MenuItemService menuItemService, MainViewModel mainViewModel)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _menuItemService = menuItemService ?? throw new ArgumentNullException(nameof(menuItemService)); // Injecteaza MenuItemService
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));


            // Initializeaza colectia
            MenuCategories = new ObservableCollection<CategoryDisplayWrapper>();

            // Initializeaza command-urile PRIMA DATA
            // Adauga CanExecute pentru a dezactiva butonul "Comenzile Mele" pentru invitati
            ShowClientOrdersCommand = new RelayCommand(ExecuteShowClientOrders, CanExecuteShowClientOrders);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            // Initializeaza noul command pentru a reveni la Login
            ShowLoginCommand = new RelayCommand(ExecuteShowLogin);
            // TODO: Initializeaza AddToCartCommand cu CanExecute
            // AddToCartCommand = new RelayCommand(ExecuteAddToCart, CanExecuteAddToCart);


            // Acum seteaza utilizatorul autentificat (aceasta va apela setter-ul IsGuest FARA eroare)
            LoggedInUser = loggedInUser;
            // Starea IsGuest este setata automat in setter-ul LoggedInUser

            // Incarca datele meniului la initializarea ViewModel-ului
            Task.Run(async () => await LoadMenuData());
        }

        // --- Metode pentru Command-uri ---

        private void ExecuteShowClientOrders(object parameter)
        {
            // TODO: Implementeaza ViewModel si View pentru comenzile clientului
            // _mainViewModel.ShowClientOrdersView(LoggedInUser); // Apeleaza o metoda in MainViewModel pentru a schimba View-ul
            ErrorMessage = "Sectiunea Comenzi Client nu este inca implementata.";
        }

        // Metoda CanExecute pentru ShowClientOrdersCommand (dezactivat pentru invitati)
        private bool CanExecuteShowClientOrders(object parameter)
        {
            // Command-ul este activ doar daca utilizatorul este autentificat (NU este invitat)
            return !IsGuest;
        }


        private void ExecuteLogout(object parameter)
        {
            _mainViewModel.Logout(); // Apeleaza metoda Logout din MainViewModel
        }

        // Metoda de executie pentru noul Command ShowLoginCommand (pentru invitati)
        private void ExecuteShowLogin(object parameter)
        {
            _mainViewModel.ShowLoginView(); // Apeleaza metoda de navigare din MainViewModel
        }


        // TODO: Implementeaza ExecuteAddToCart(object parameter) si CanExecuteAddToCart(object parameter)
        /*
        private void ExecuteAddToCart(object parameter)
        {
            // Logica de adaugare in cos
            if (parameter is DisplayMenuItem item)
            {
                // TODO: Adauga item-ul in cos (o colectie in ViewModel sau un alt serviciu)
                Debug.WriteLine($"Added {item.Name} to cart.");
                SuccessMessage = $"{item.Name} a fost adaugat in cos.";
            }
        }

        private bool CanExecuteAddToCart(object parameter)
        {
             // Command-ul este activ doar daca NU este invitat
             return !IsGuest;
        }
        */


        // --- Metoda pentru incarcarea datelor meniului ---

        private async Task LoadMenuData()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                // Verifica daca serviciile sunt null (cazul design-time)
                if (_categoryService == null || _dishService == null || _menuItemService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real menu data.");
                    // Poti adauga date mock aici pentru design-time
                    // Adauga date mock in colectia de tip CategoryDisplayWrapper
                    // MenuCategories.Clear();
                    // var mockCategory = new CategoryDisplayWrapper(new Category { Name = "Mock Categorie 1" });
                    // mockCategory.DisplayItems.Add(new DisplayMenuItem(new Dish { Name = "Mock Dish 1", Price = 10m }));
                    // MenuCategories.Add(mockCategory);
                    return;
                }

                // 1. Incarca toate Categoriile
                var categories = await _categoryService.GetAllCategoriesAsync();

                // 2. Incarca toate Preparatele (Dish) - asigura-te ca serviciul le include si Categoria
                var dishes = await _dishService.GetAllDishesAsync(); // Asigura-te ca GetAllDishesAsync include Category

                // 3. Incarca toate Meniurile (MenuItem) - asigura-te ca serviciul le include si Categoria si MenuItemDishes (si Dish-urile din ele daca este necesar)
                var menuItems = await _menuItemService.GetAllMenuItemsAsync(); // Asigura-te ca GetAllMenuItemsAsync include Category


                // Organizeaza Preparatele si Meniurile pe categorii
                var categoriesWithItems = new ObservableCollection<CategoryDisplayWrapper>();

                foreach (var category in categories)
                {
                    var categoryWrapper = new CategoryDisplayWrapper(category);

                    // Adauga preparatele din aceasta categorie
                    var dishesInCategory = dishes.Where(d => d.CategoryId == category.Id).ToList();
                    foreach (var dish in dishesInCategory)
                    {
                        categoryWrapper.DisplayItems.Add(new DisplayMenuItem(dish));
                    }

                    // Adauga meniurile din aceasta categorie
                    var menuItemsInCategory = menuItems.Where(mi => mi.CategoryId == category.Id).ToList();
                    foreach (var menuItem in menuItemsInCategory)
                    {
                        categoryWrapper.DisplayItems.Add(new DisplayMenuItem(menuItem));
                    }

                    // Adauga wrapper-ul categoriei la colectia principala DOAR daca are itemi
                    if (categoryWrapper.DisplayItems.Any())
                    {
                        categoriesWithItems.Add(categoryWrapper);
                    }
                }

                // Seteaza colectia principala a ViewModel-ului
                // FIX: Atribuie colectia nou creata proprietatii
                MenuCategories = categoriesWithItems;
                OnPropertyChanged(nameof(MenuCategories)); // Notifica View-ul ca s-a schimbat colectia


                SuccessMessage = "Meniul a fost incarcat cu succes.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea meniului: {ex.Message}";
                Debug.WriteLine($"Eroare la incarcarea meniului: {ex.Message}");
            }
        }
    }

    // Clasa wrapper pentru a afisa Categoriile impreuna cu itemii lor (Dish/MenuItem)
    // Ramane in acelasi namespace sau in Models.Wrappers, dar trebuie sa fie public
    public class CategoryDisplayWrapper : ViewModelBase // Inherit ViewModelBase for potential future needs
    {
        public Category Category { get; set; }
        public ObservableCollection<Models.Wrappers.DisplayMenuItem> DisplayItems { get; set; } // Use the DisplayMenuItem wrapper

        public CategoryDisplayWrapper(Category category)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            DisplayItems = new ObservableCollection<Models.Wrappers.DisplayMenuItem>();
        }
    }
}
