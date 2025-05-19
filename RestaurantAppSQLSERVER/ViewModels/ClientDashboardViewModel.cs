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
using Microsoft.EntityFrameworkCore; // Necesara pentru FromSqlInterpolated

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
                // Notificare explicita pentru command-uri care depind de IsGuest (ex: ShowClientOrdersCommand, AddToCartCommand, ShowLoginCommand)
                ((RelayCommand)ShowClientOrdersCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand)ShowLoginCommand)?.RaiseCanExecuteChanged(); // Notifica si ShowLoginCommand
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


        // Serviciile necesare (injectate) - Pastram referintele chiar daca folosim SP pentru incarcarea meniului
        // Daca vei folosi SP pentru Plasare Comanda, vei avea nevoie de OrderService
        private readonly CategoryService _categoryService; // Inca util pentru alte operatii sau daca nu folosesti SP complet
        private readonly DishService _dishService; // Inca util pentru alte operatii
        private readonly MenuItemService _menuItemService; // Inca util pentru alte operatii
        private readonly MainViewModel _mainViewModel; // Referenta catre MainViewModel pentru navigare/logout


        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        public ClientDashboardViewModel() : this(null, null, null, null, null)
        {
            Debug.WriteLine("ClientDashboardViewModel created for Design Time.");
            // Poti adauga date mock aici pentru a vedea ceva in designer
            // LoggedInUser = new User { Nume = "Client Mock" }; // Foloseste Nume conform entitatii User
            // IsGuest = false; // Seteaza starea pentru design-time daca vrei sa vezi UI-ul de client
            // Initializeaza colectia pentru design time
            MenuCategories = new ObservableCollection<CategoryDisplayWrapper>();
            var mockCategory = new CategoryDisplayWrapper(new Category { Name = "Mock Categorie 1" });
            // Foloseste constructorul gol al DisplayMenuItem pentru a crea obiecte mock
            mockCategory.DisplayItems.Add(new DisplayMenuItem { ItemName = "Mock Dish 1", ItemPrice = 10m, ItemType = "Dish", QuantityDisplay = "250g", AllergensString = "Gluten" });
            mockCategory.DisplayItems.Add(new DisplayMenuItem { ItemName = "Mock Meniu 1", ItemPrice = 25m, ItemType = "MenuItem", QuantityDisplay = "Meniu", MenuItemComponentsString = "1x Mock Dish 1; 1x Mock Dish 2", AllergensString = "Gluten, Lactoza" });
            MenuCategories.Add(mockCategory);
        }


        // Constructorul principal - folosit la RULARE
        // Adaugam un parametru boolean isGuest
        public ClientDashboardViewModel(User loggedInUser, CategoryService categoryService, DishService dishService, MenuItemService menuItemService, MainViewModel mainViewModel)
        {
            // Initializeaza colectia INAINTE de a seta LoggedInUser
            MenuCategories = new ObservableCollection<CategoryDisplayWrapper>();

            // Initializeaza command-urile PRIMA DATA
            ShowClientOrdersCommand = new RelayCommand(ExecuteShowClientOrders, CanExecuteShowClientOrders);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            // Initializeaza noul command pentru a reveni la Login
            ShowLoginCommand = new RelayCommand(ExecuteShowLogin, CanExecuteShowLogin); // Adauga CanExecute si pentru ShowLoginCommand
            // TODO: Initializeaza AddToCartCommand cu CanExecute
            // AddToCartCommand = new RelayCommand(ExecuteAddToCart, CanExecuteAddToCart);

            // Seteaza utilizatorul autentificat (aceasta va apela setter-ul IsGuest)
            LoggedInUser = loggedInUser;
            // Starea IsGuest este setata automat in setter-ul LoggedInUser

            // Injecteaza serviciile (chiar daca folosim SP pentru incarcarea meniului, serviciile pot fi utile pentru alte operatii)
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _menuItemService = menuItemService ?? throw new ArgumentNullException(nameof(menuItemService));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));


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

        // Metoda CanExecute pentru ShowLoginCommand (activ doar pentru invitati)
        private bool CanExecuteShowLogin(object parameter)
        {
            // Command-ul este activ doar daca utilizatorul ESTE invitat
            return IsGuest;
        }


        // TODO: Implementeaza ExecuteAddToCart(object parameter) si CanExecuteAddToCart(object parameter)
        /*
        private void ExecuteAddToCart(object parameter)
        {
            // Logica de adaugare in cos
            if (parameter is DisplayMenuItem item)
            {
                // TODO: Adauga item-ul in cos (o colectie in ViewModel sau un alt serviciu)
                Debug.WriteLine($"Added {item.ItemName} to cart."); // Foloseste ItemName
                SuccessMessage = $"{item.ItemName} a fost adaugat in cos.";
            }
        }

        private bool CanExecuteAddToCart(object parameter)
        {
             // Command-ul este activ doar daca NU este invitat
             return !IsGuest;
        }
        */


        // --- Metoda pentru incarcarea datelor meniului folosind Procedura Stocata ---

        private async Task LoadMenuData()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                // Folosim DbContextFactory pentru a crea un context nou
                // Accesam _dbContextFactory prin MainViewModel (acum este public)
                using (var context = _mainViewModel._dbContextFactory.CreateDbContext())
                {
                    // Apeleaza procedura stocata GetFullMenuDetails
                    // Mapam rezultatele la o lista de obiecte DisplayMenuItem
                    var menuItemsData = await context.Set<DisplayMenuItem>() // Folosim Set<DisplayMenuItem>() pentru a mapa la un tip fara cheie
                                                   .FromSqlRaw("EXEC GetFullMenuDetails") // Apelam procedura stocata bruta
                                                   .ToListAsync(); // Executa query-ul si aduce rezultatele

                    // Organizeaza datele pe categorii
                    var categoriesWithItems = new ObservableCollection<CategoryDisplayWrapper>();

                    // Gruparea se face acum in C# pe baza CategoryId si CategoryName returnate de SP
                    var groupedItems = menuItemsData.GroupBy(item => new { item.CategoryId, item.CategoryName });

                    foreach (var group in groupedItems)
                    {
                        // Creeaza un wrapper pentru fiecare categorie
                        // Asiguram ca Category este initializat corect
                        var categoryWrapper = new CategoryDisplayWrapper(new Category { Id = group.Key.CategoryId, Name = group.Key.CategoryName });

                        // Adauga itemii (Dish/MenuItem) din grupul curent la wrapper-ul categoriei
                        foreach (var item in group.OrderBy(item => item.ItemName)) // Ordoneaza itemii in cadrul categoriei
                        {
                            categoryWrapper.DisplayItems.Add(item); // Adauga direct obiectul DisplayMenuItem mapat din SP
                        }

                        // Adauga wrapper-ul categoriei la colectia principala
                        categoriesWithItems.Add(categoryWrapper);
                    }

                    // Sorteaza categoriile (optional)
                    MenuCategories = new ObservableCollection<CategoryDisplayWrapper>(categoriesWithItems.OrderBy(c => c.Category.Name));

                    OnPropertyChanged(nameof(MenuCategories)); // Notifica View-ul ca s-a schimbat colectia

                    SuccessMessage = "Meniul a fost incarcat cu succes.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea meniului: {ex.Message}";
                Debug.WriteLine($"Eroare la incarcarea meniului (SP): {ex.Message}");
            }
        }
    }

    // Clasa wrapper pentru a afisa Categoriile impreuna cu itemii lor (Dish/MenuItem)
    // Ramane in acelasi namespace sau in Models.Wrappers, dar trebuie sa fie public
    public class CategoryDisplayWrapper : ViewModelBase // Inherit ViewModelBase for potential future needs
    {
        public Category Category { get; set; }
        public ObservableCollection<Models.Wrappers.DisplayMenuItem> DisplayItems { get; set; } // Use the DisplayMenuItem wrapper

        // Constructor principal
        public CategoryDisplayWrapper(Category category)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            DisplayItems = new ObservableCollection<Models.Wrappers.DisplayMenuItem>();
        }

        // Constructor gol pentru design time sau mapare (optional)
        public CategoryDisplayWrapper()
        {
            Category = new Category(); // Initializeaza Category
            DisplayItems = new ObservableCollection<Models.Wrappers.DisplayMenuItem>();
        }
    }
}
