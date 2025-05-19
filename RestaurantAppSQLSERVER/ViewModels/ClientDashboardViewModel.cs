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
    // ViewModel principal pentru Dashboard-ul Clientului
    public class ClientDashboardViewModel : ViewModelBase
    {
        // Colectie pentru afisarea meniului grupat pe categorii
        // Fiecare CategoryDisplayWrapper va contine o Categorie si o lista de DisplayMenuItem (Dish sau MenuItem)
        // FIX: Schimba tipul colectiei la ObservableCollection<CategoryDisplayWrapper>
        public ObservableCollection<CategoryDisplayWrapper> MenuCategories { get; set; }


        // Proprietate pentru a afisa informatii despre utilizatorul autentificat
        private User _loggedInUser;
        public User LoggedInUser
        {
            get => _loggedInUser;
            set
            {
                _loggedInUser = value;
                OnPropertyChanged(nameof(LoggedInUser));
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


        private readonly CategoryService _categoryService;
        private readonly DishService _dishService;
        private readonly MenuItemService _menuItemService; // Serviciul principal pentru MenuItem (Meniu)
        private readonly MainViewModel _mainViewModel; // Referinta catre MainViewModel pentru navigare/logout


        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        public ClientDashboardViewModel() : this(null, null, null, null, null)
        {
            // Poti adauga aici date mock pentru a vedea ceva in designer
            // LoggedInUser = new User { Username = "Client Mock" };
            // Initializeaza colectia pentru design time
            // MenuCategories = new ObservableCollection<CategoryDisplayWrapper>();
            // MenuCategories.Add(new CategoryDisplayWrapper(new Category { Name = "Mock Categorie 1" }));
            // MenuCategories[0].DisplayItems.Add(new DisplayMenuItem(new Dish { Name = "Mock Dish 1", Price = 10m }));
        }


        // Constructorul principal - folosit la RULARE
        public ClientDashboardViewModel(User loggedInUser, CategoryService categoryService, DishService dishService, MenuItemService menuItemService, MainViewModel mainViewModel)
        {
            LoggedInUser = loggedInUser ?? throw new ArgumentNullException(nameof(loggedInUser));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _menuItemService = menuItemService ?? throw new ArgumentNullException(nameof(menuItemService)); // Injecteaza MenuItemService
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));


            // Initializeaza colectia
            // FIX: Initializeaza colectia cu tipul corect
            MenuCategories = new ObservableCollection<CategoryDisplayWrapper>();

            // Initializeaza command-urile
            ShowClientOrdersCommand = new RelayCommand(ExecuteShowClientOrders);
            LogoutCommand = new RelayCommand(ExecuteLogout);


            // Incarca datele meniului la initializarea ViewModel-ului
            // Folosim Task.Run pentru a nu bloca thread-ul UI
            Task.Run(async () => await LoadMenuData());
        }

        // --- Metode pentru Command-uri ---

        private void ExecuteShowClientOrders(object parameter)
        {
            // TODO: Implementeaza ViewModel si View pentru comenzile clientului
            // _mainViewModel.ShowClientOrdersView(LoggedInUser); // Apeleaza o metoda in MainViewModel pentru a schimba View-ul
            ErrorMessage = "Sectiunea Comenzi Client nu este inca implementata.";
        }

        private void ExecuteLogout(object parameter)
        {
            _mainViewModel.Logout(); // Apeleaza metoda Logout din MainViewModel
        }


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


                // Curata colectia existenta
                // MenuCategories.Clear(); // Nu mai este necesar sa curatam aici, cream o colectie noua

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
