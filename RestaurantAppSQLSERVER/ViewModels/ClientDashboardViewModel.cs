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
using Microsoft.Extensions.Configuration; // Necesara pentru configurare (appsettings.json)
using System.Windows; // Adaugat pentru App.Current.Dispatcher


namespace RestaurantAppSQLSERVER.ViewModels
{
    // ViewModel principal pentru Dashboard-ul Clientului (si Invitatului)
    public class ClientDashboardViewModel : ViewModelBase
    {
        // Colectie pentru afisarea meniului grupat pe categorii (aceasta va fi cea filtrata)
        public ObservableCollection<CategoryDisplayWrapper> MenuCategories { get; set; }

        // Colectie privata pentru a pastra meniul complet nefiltrat
        private List<CategoryDisplayWrapper> _fullMenuCategories;

        // Colectie pentru alergeni (pentru filtrare)
        public ObservableCollection<Allergen> Allergens { get; set; }


        // --- Proprietati si Colectii pentru COSUL DE CUMPARATURI ---
        // Colectie pentru itemii din cos
        public ObservableCollection<CartItem> ShoppingCart { get; set; }

        // Proprietati pentru totalurile cosului
        private decimal _cartSubtotal;
        public decimal CartSubtotal
        {
            get => _cartSubtotal;
            set
            {
                _cartSubtotal = value;
                OnPropertyChanged(nameof(CartSubtotal));
                // CORECTAT: Apelam CalculateDiscountAndTransport() si CalculateFinalTotals() aici
                // CalculateDiscountAndTransport() va apela CalculateFinalTotals() la final
                // Acest lucru asigura ca reducerile si totalul se actualizeaza imediat ce subtotalul se schimba (ex: adaugi/elimini itemi)
                // Nu mai este nevoie sa apelam CalculateFinalTotals() separat in setter.
                CalculateDiscountAndTransport();

                // Notifica PlaceOrderCommand ca starea CanExecute s-ar putea schimba (cosul nu mai e gol)
                ((RelayCommand)PlaceOrderCommand)?.RaiseCanExecuteChanged();
            }
        }

        private decimal _discountAmount;
        public decimal DiscountAmount
        {
            get => _discountAmount;
            set
            {
                _discountAmount = value;
                OnPropertyChanged(nameof(DiscountAmount));
                CalculateFinalTotals(); // Recalculeaza totalurile cand reducerea se schimba
            }
        }

        private decimal _transportCost;
        public decimal TransportCost
        {
            get => _transportCost;
            set
            {
                _transportCost = value;
                OnPropertyChanged(nameof(TransportCost));
                CalculateFinalTotals(); // Recalculeaza totalurile cand transportul se schimba
            }
        }

        private decimal _cartTotal;
        public decimal CartTotal
        {
            get => _cartTotal;
            set
            {
                _cartTotal = value;
                OnPropertyChanged(nameof(CartTotal));
            }
        }
        // --- SFARSIT Proprietati si Colectii pentru COSUL DE CUMPARATURI ---


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
                // Notificare explicita pentru command-uri care depind de IsGuest (ex: ShowClientOrdersCommand, AddToCartCommand, PlaceOrderCommand, ShowLoginCommand)
                ((RelayCommand)ShowClientOrdersCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand)AddToCartCommand)?.RaiseCanExecuteChanged(); // Adaugat
                ((RelayCommand)PlaceOrderCommand)?.RaiseCanExecuteChanged(); // Adaugat
                ((RelayCommand)ShowLoginCommand)?.RaiseCanExecuteChanged(); // Adaugat
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

                // Daca utilizatorul se schimba (ex: de la invitat la autentificat),
                // recalculeaza discount-ul si transportul (pentru logica de loialitate)
                // Ruleaza intr-un Task separat pentru a nu bloca UI-ul
                Task.Run(() => CalculateDiscountAndTransport());
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

        // --- Proprietati pentru functionalitatea de cautare ---
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                // Optional: declanseaza cautarea automat la fiecare modificare a textului
                // ExecuteSearch(null);
            }
        }

        private SearchType _selectedSearchType;
        public SearchType SelectedSearchType
        {
            get => _selectedSearchType;
            set
            {
                _selectedSearchType = value;
                OnPropertyChanged(nameof(SelectedSearchType));
                // Curata textul de cautare si alergenul selectat cand se schimba tipul de cautare
                SearchText = string.Empty;
                SelectedAllergen = null;
                // Optional: declanseaza cautarea automat cand se schimba tipul
                // ExecuteSearch(null);
            }
        }

        private Allergen _selectedAllergen;
        public Allergen SelectedAllergen
        {
            get => _selectedAllergen;
            set
            {
                _selectedAllergen = value;
                OnPropertyChanged(nameof(SelectedAllergen));
                // Optional: declanseaza cautarea automat cand se schimba alergenul
                // ExecuteSearch(null);
            }
        }

        private bool _includeAllergen = true; // True pentru "Contine", False pentru "Nu contine" pentru alergeni
        public bool IncludeAllergen
        {
            get => _includeAllergen;
            set
            {
                _includeAllergen = value;
                OnPropertyChanged(nameof(IncludeAllergen));
                // Optional: declanseaza cautarea automat cand se schimba optiunea include/exclude
                // ExecuteSearch(null);
            }
        }

        // NOU: Proprietate pentru optiunea "Contine" / "Nu contine" pentru cautarea dupa nume
        private bool _includeName = true; // True pentru "Contine", False pentru "Nu contine" pentru nume
        public bool IncludeName
        {
            get => _includeName;
            set
            {
                _includeName = value;
                OnPropertyChanged(nameof(IncludeName));
                // Optional: declanseaza cautarea automat cand se schimba optiunea include/exclude
                // ExecuteSearch(null);
            }
        }


        // Enum pentru tipurile de cautare
        public enum SearchType
        {
            Nume,
            Alergen
        }
        // --- SFARSIT Proprietati pentru functionalitatea de cautare ---


        // Command-uri pentru navigare (ex: catre sectiunea Comenzi Client)
        public ICommand ShowClientOrdersCommand { get; }
        public ICommand LogoutCommand { get; } // Command pentru Logout
        // Noul Command pentru a naviga inapoi la Login (pentru invitati)
        public ICommand ShowLoginCommand { get; }
        // Command pentru a declansa cautarea
        public ICommand SearchCommand { get; }


        // --- Command-uri pentru COS si Comanda ---
        public ICommand AddToCartCommand { get; } // Command pentru adaugare in cos
        public ICommand RemoveFromCartCommand { get; } // Command pentru eliminare din cos (optional, dar util)
        public ICommand PlaceOrderCommand { get; } // Command pentru plasarea comenzii
        // --- SFARSIT Command-uri pentru COS si Comanda ---


        // Serviciile necesare (injectate)
        private readonly CategoryService _categoryService;
        private readonly DishService _dishService;
        private readonly MenuItemService _menuItemService; // Serviciul principal pentru MenuItem (Meniu)
        private readonly OrderService _orderService; // Serviciul pentru Comenzi - ACUM IL FOLOSIM PENTRU PLACEORDER
        private readonly AllergenService _allergenService; // NOU: Serviciul pentru Alergeni
        private readonly MainViewModel _mainViewModel; // Referenta catre MainViewModel pentru navigare/logout
        private readonly IConfiguration _configuration; // Injecteaza configuratia pentru a citi din appsettings.json


        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        public ClientDashboardViewModel() : this(null, null, null, null, null, null, null, null) // Adaugat un null pentru AllergenService si IConfiguration
        {
            Debug.WriteLine("ClientDashboardViewModel created for Design Time.");
            // Poti adauga date mock aici pentru a vedea ceva in designer
            // Initializeaza colectiile pentru design time
            MenuCategories = new ObservableCollection<CategoryDisplayWrapper>();
            _fullMenuCategories = new List<CategoryDisplayWrapper>(); // Initializeaza si lista completa mock
            ShoppingCart = new ObservableCollection<CartItem>();
            Allergens = new ObservableCollection<Allergen>(); // Initializeaza alergenii mock

            var mockCategory = new CategoryDisplayWrapper(new Category { Name = "Mock Categorie 1" });
            // Foloseste constructorul gol al DisplayMenuItem pentru a crea obiecte mock
            var mockDish = new DisplayMenuItem { ItemId = 1, ItemName = "Mock Dish 1", ItemPrice = 10m, ItemType = "Dish", QuantityDisplay = "250g", AllergensString = "Gluten, Lactoza" };
            var mockMenuItem = new DisplayMenuItem { ItemId = 10, ItemName = "Mock Meniu 1", ItemPrice = 25m, ItemType = "MenuItem", QuantityDisplay = "Meniu", MenuItemComponentsString = "1x Mock Dish 1; 1x Mock Dish 2", AllergensString = "Gluten, Nuci" };
            mockCategory.DisplayItems.Add(mockDish);
            mockCategory.DisplayItems.Add(mockMenuItem);
            MenuCategories.Add(mockCategory);
            _fullMenuCategories.Add(mockCategory); // Adauga si la lista completa mock

            Allergens.Add(new Allergen { Id = 1, Name = "Gluten" });
            Allergens.Add(new Allergen { Id = 2, Name = "Lactoza" });
            Allergens.Add(new Allergen { Id = 3, Name = "Nuci" });

            // Seteaza tipul de cautare implicit pentru design time
            SelectedSearchType = SearchType.Nume;


            // Adauga itemi mock in cos pentru design time
            ShoppingCart.Add(new CartItem(mockDish, 2)); // 2 bucati din Mock Dish 1
            ShoppingCart.Add(new CartItem(mockMenuItem, 1)); // 1 bucata din Mock Meniu 1
            CalculateCartSubtotal(); // Calculeaza subtotalul mock
                                     // Seteaza valori mock pentru discount si transport (daca vrei sa le vezi in designer)
            DiscountAmount = 2.50m;
            TransportCost = 15.00m;
            CalculateFinalTotals(); // Calculeaza totalul mock
        }


        // Constructorul principal - folosit la RULARE
        // Adaugam un parametru boolean isGuest
        public ClientDashboardViewModel(User loggedInUser, CategoryService categoryService, DishService dishService, MenuItemService menuItemService, OrderService orderService, AllergenService allergenService, MainViewModel mainViewModel, IConfiguration configuration) // Injecteaza AllergenService si IConfiguration
        {
            // Initializeaza colectiile INAINTE de a seta LoggedInUser
            MenuCategories = new ObservableCollection<CategoryDisplayWrapper>();
            _fullMenuCategories = new List<CategoryDisplayWrapper>(); // Initializeaza lista completa
            ShoppingCart = new ObservableCollection<CartItem>(); // Initializeaza cosul
            Allergens = new ObservableCollection<Allergen>(); // Initializeaza colectia de alergeni

            // Initializeaza command-urile PRIMA DATA
            ShowClientOrdersCommand = new RelayCommand(ExecuteShowClientOrders, CanExecuteShowClientOrders);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            ShowLoginCommand = new RelayCommand(ExecuteShowLogin, CanExecuteShowLogin);
            // --- Initializeaza Command-urile pentru COS si Comanda ---
            AddToCartCommand = new RelayCommand(ExecuteAddToCart, CanExecuteAddToCart); // Adaugat
            RemoveFromCartCommand = new RelayCommand(ExecuteRemoveFromCart); // Adaugat (CanExecute poate fi adaugat ulterior)
            PlaceOrderCommand = new RelayCommand(ExecutePlaceOrder, CanExecutePlaceOrder); // Adaugat
            // --- Initializeaza Command-ul pentru cautare ---
            SearchCommand = new RelayCommand(ExecuteSearch);


            // Injecteaza serviciile si configuratia
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _menuItemService = menuItemService ?? throw new ArgumentNullException(nameof(menuItemService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService)); // Injecteaza OrderService
            _allergenService = allergenService ?? throw new ArgumentNullException(nameof(allergenService)); // NOU: Injecteaza AllergenService
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(_configuration)); // Injecteaza IConfiguration


            // Acum seteaza utilizatorul autentificat (aceasta va apela setter-ul IsGuest)
            LoggedInUser = loggedInUser;
            // Starea IsGuest este setata automat in setter-ul LoggedInUser

            // Seteaza tipul de cautare implicit la Nume la pornire
            SelectedSearchType = SearchType.Nume;

            // Incarca datele meniului si alergenii la initializarea ViewModel-ului
            Task.Run(async () => await LoadInitialData());

            // Calculeaza totalurile initiale (vor fi 0 la pornire)
            CalculateCartSubtotal();
            // CalculateDiscountAndTransport(); // Nu mai apelam aici, este apelat din setter-ul CartSubtotal si LoggedInUser
            // CalculateFinalTotals(); // Nu mai apelam aici, este apelat din CalculateDiscountAndTransport()
        }

        // --- Metode pentru Command-uri ---

        // Metoda de executie pentru ShowClientOrdersCommand
        private void ExecuteShowClientOrders(object parameter)
        {
            // Apeleaza metoda din MainViewModel pentru a schimba View-ul catre istoricul comenzilor
            _mainViewModel.ShowClientOrdersView(LoggedInUser); // Pasam utilizatorul autentificat
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

        // Metoda de executie pentru SearchCommand
        private void ExecuteSearch(object parameter)
        {
            // Daca parametrul este "Reset", reseteaza campurile de cautare
            if (parameter is string paramString && paramString == "Reset")
            {
                SearchText = string.Empty;
                SelectedSearchType = SearchType.Nume; // Reseteaza la cautare dupa nume
                SelectedAllergen = null;
                IncludeAllergen = true;
                IncludeName = true; // Reseteaza si optiunea pentru nume
            }

            FilterMenu(); // Apeleaza metoda de filtrare
        }


        // --- Metode pentru Command-urile COS si Comanda ---

        // Metoda de executie pentru AddToCartCommand
        private void ExecuteAddToCart(object parameter)
        {
            ErrorMessage = string.Empty; // Curata mesajele de eroare
            SuccessMessage = string.Empty; // Curata mesajele de succes

            if (parameter is DisplayMenuItem item)
            {
                // Cauta daca item-ul exista deja in cos
                var existingCartItem = ShoppingCart.FirstOrDefault(ci => ci.Item.ItemId == item.ItemId && ci.Item.ItemType == item.ItemType);

                if (existingCartItem != null)
                {
                    // Item-ul exista, creste cantitatea
                    existingCartItem.Quantity++;
                    // Notifica UI-ul ca proprietatea Quantity s-a schimbat (daca CartItem implementeaza INotifyPropertyChanged)
                    // Sau, daca nu implementeaza, poti inlocui item-ul in colectie pentru a forta refresh-ul
                    // ShoppingCart.Remove(existingCartItem);
                    // ShoppingCart.Add(existingCartItem); // Aceasta poate schimba ordinea
                    // O alternativa mai buna este sa te asiguri ca CartItem notifica schimbarea Quantity
                }
                else
                {
                    // Item-ul nu exista, adauga-l in cos cu cantitatea 1
                    ShoppingCart.Add(new CartItem(item, 1));
                }

                CalculateCartSubtotal(); // Recalculeaza subtotalul cosului
                SuccessMessage = $"{item.ItemName} a fost adaugat in cos.";
            }
        }

        // Metoda CanExecute pentru AddToCartCommand (dezactivat pentru invitati)
        private bool CanExecuteAddToCart(object parameter)
        {
            // Command-ul este activ doar daca NU este invitat
            return !IsGuest;
        }

        // Metoda de executie pentru RemoveFromCartCommand
        private void ExecuteRemoveFromCart(object parameter)
        {
            ErrorMessage = string.Empty; // Curata mesajele de eroare
            SuccessMessage = string.Empty; // Curata mesajele de succes

            if (parameter is CartItem itemToRemove)
            {
                if (itemToRemove.Quantity > 1)
                {
                    // Daca sunt mai multe bucati, scade cantitatea
                    itemToRemove.Quantity--;
                    // Notifica UI-ul ca proprietatea Quantity s-a schimbat (daca CartItem implementeaza INotifyPropertyChanged)
                }
                else
                {
                    // Daca este o singura bucata, elimina item-ul din cos
                    ShoppingCart.Remove(itemToRemove);
                }

                CalculateCartSubtotal(); // Recalculeaza subtotalul cosului
                SuccessMessage = $"{itemToRemove.Item.ItemName} a fost eliminat din cos.";
            }
        }

        // Metoda de executie pentru PlaceOrderCommand (async)
        private async void ExecutePlaceOrder(object parameter)
        {
            ErrorMessage = string.Empty; // Curata mesajele de eroare
            SuccessMessage = string.Empty; // Curata mesajele de succes

            // Verifica daca utilizatorul este autentificat si cosul nu este gol
            if (IsGuest || LoggedInUser == null)
            {
                ErrorMessage = "Trebuie sa fiti autentificat pentru a plasa o comanda.";
                return;
            }

            if (!ShoppingCart.Any())
            {
                ErrorMessage = "Cosul de cumparaturi este gol.";
                return;
            }

            try
            {
                // 1. Calculeaza Reducerea si Transportul pe baza regulilor si setarilor din appsettings.json
                // Aceasta logica ar putea fi intr-un alt service dedicat (ex: PricingService)
                // Pentru simplitate, o implementam aici in ViewModel.
                // CalculateDiscountAndTransport(); // Nu mai apelam aici, este apelat din setter-ul CartSubtotal si LoggedInUser

                // 2. Pregateste lista de OrderItemData pentru SP din itemii din cos
                var orderItemsData = ShoppingCart.Select(cartItem => new OrderItemData
                {
                    ItemId = cartItem.Item.ItemId,
                    ItemType = cartItem.Item.ItemType,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Item.ItemPrice, // Folosim pretul itemului la momentul adaugarii in cos
                    ItemName = cartItem.Item.ItemName // Folosim numele itemului la momentul adaugarii in cos
                }).ToList();

                // 3. Apeleaza OrderService pentru a plasa comanda folosind SP
                var result = await _orderService.PlaceOrderAsync(
                    LoggedInUser.Id,
                    DiscountAmount, // Suma reducerii calculate
                    TransportCost, // Costul transportului calculat
                    orderItemsData // Lista de itemi din cos mapata la OrderItemData
                );

                // 4. Gestioneaza rezultatul
                if (result.IsSuccess)
                {
                    SuccessMessage = result.Message + $" Cod comanda: {result.OrderId}"; // Afiseaza ID-ul comenzii returnat de SP
                    ShoppingCart.Clear(); // Goleste cosul dupa plasarea cu succes
                    CalculateCartSubtotal(); // Reseteaza totalurile (va declansa recalcularea discount/transport/total)
                    // CalculateDiscountAndTransport(); // Nu mai apelam aici
                    // CalculateFinalTotals(); // Nu mai apelam aici
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"A aparut o eroare la plasarea comenzii: {ex.Message}";
                Debug.WriteLine($"Place Order Error: {ex.Message}");
            }
        }

        // Metoda CanExecute pentru PlaceOrderCommand (dezactivat pentru invitati si cos gol)
        private bool CanExecutePlaceOrder(object parameter)
        {
            // Command-ul este activ doar daca NU este invitat SI cosul NU este gol
            return !IsGuest && ShoppingCart.Any();
        }

        // --- Metode Helper pentru Calculul Totalurilor Cosului ---

        // Calculeaza subtotalul cosului (suma preturilor itemilor * cantitatea lor)
        private void CalculateCartSubtotal()
        {
            CartSubtotal = ShoppingCart.Sum(item => item.Item.ItemPrice * item.Quantity);
            // CalculateDiscountAndTransport() si CalculateFinalTotals() sunt apelate din setter-ul CartSubtotal
        }

        // Calculeaza reducerea si costul transportului pe baza setarilor si a subtotalului cosului
        // Aceasta metoda este acum ASYNC pentru a apela OrderService
        private async Task CalculateDiscountAndTransport()
        {
            // Citeste setarile din appsettings.json
            // Asigura-te ca ai adaugat pachetul NuGet Microsoft.Extensions.Configuration.Binder
            // pentru a folosi GetSection(...).Get<T>() sau GetValue<T>()
            // Verifica daca _configuration nu este null (cazul design-time)
            if (_configuration == null) return;

            var discountThresholdAmount = _configuration.GetValue<decimal>("OrderSettings:DiscountThresholdAmount");
            var loyaltyOrderCountThreshold = _configuration.GetValue<int>("OrderSettings:LoyaltyOrderCountThreshold");
            var loyaltyTimeIntervalDays = _configuration.GetValue<int>("OrderSettings:LoyaltyTimeIntervalDays");
            var discountPercentage = _configuration.GetValue<decimal>("OrderSettings:DiscountPercentage"); // Procentul (ex: 10 pentru 10%)
            var freeTransportThreshold = _configuration.GetValue<decimal>("OrderSettings:FreeTransportThreshold");
            var transportCostValue = _configuration.GetValue<decimal>("OrderSettings:TransportCost");


            // Logica de calcul a reducerii
            decimal calculatedDiscountAmount = 0;
            bool applyDiscount = false;

            // Conditia 1: Comanda mai mare decat o anumita suma (y)
            if (CartSubtotal > discountThresholdAmount)
            {
                applyDiscount = true;
            }

            // Conditia 2: Mai mult de z comenzi in intervalul t de timp (doar pentru utilizatori autentificati)
            // Implementam logica de verificare a istoricului comenzilor aici
            if (!IsGuest && LoggedInUser != null && _orderService != null) // Verifica daca userul e autentificat si OrderService e disponibil
            {
                try
                {
                    // Apeleaza OrderService pentru a numara comenzile in intervalul de timp
                    var recentOrderCount = await _orderService.GetOrderCountInTimeFrameAsync(LoggedInUser.Id, loyaltyTimeIntervalDays);

                    // Daca numarul de comenzi recente depaseste pragul de loialitate
                    if (recentOrderCount >= loyaltyOrderCountThreshold)
                    {
                        applyDiscount = true; // Aplica reducerea de loialitate
                        Debug.WriteLine($"Loyalty discount applied for user {LoggedInUser.Id}. Orders in last {loyaltyTimeIntervalDays} days: {recentOrderCount}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error checking loyalty discount: {ex.Message}");
                    // Continua fara a aplica discount-ul de loialitate in caz de eroare
                }
            }


            // Daca se aplica reducerea (fie prin subtotal, fie prin loialitate)
            if (applyDiscount)
            {
                calculatedDiscountAmount = CartSubtotal * (discountPercentage / 100m);
            }

            // Asigura-te ca reducerea calculata nu depaseste subtotalul
            if (calculatedDiscountAmount > CartSubtotal)
            {
                calculatedDiscountAmount = CartSubtotal;
            }

            DiscountAmount = calculatedDiscountAmount; // Seteaza suma reducerii calculate


            // Logica de calcul a costului transportului
            decimal calculatedTransportCost = 0;
            // Transport gratuit daca valoarea finala (Subtotal - Reducere) este mai mare decat pragul (a)
            // NOTA: Cerinta spune "valoare mai mica decat a lei vor plati", deci transport gratuit peste prag
            if ((CartSubtotal - DiscountAmount) < freeTransportThreshold)
            {
                calculatedTransportCost = transportCostValue;
            }

            TransportCost = calculatedTransportCost; // Seteaza costul transportului calculat

            // Recalculeaza totalul final dupa ce discountul si transportul au fost calculate
            CalculateFinalTotals();
        }


        // Calculeaza totalul final al cosului (Subtotal - Reducere + Transport)
        private void CalculateFinalTotals()
        {
            CartTotal = CartSubtotal - DiscountAmount + TransportCost;
            // Asigura-te ca totalul nu este negativ
            if (CartTotal < 0) CartTotal = 0;
        }

        // --- Metode pentru incarcarea datelor initiale (Meniu si Alergeni) ---
        private async Task LoadInitialData()
        {
            await LoadMenuData();
            await LoadAllergens();
        }


        // --- Metoda pentru incarcarea datelor meniului folosind Procedura Stocata ---

        private async Task LoadMenuData()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                // Verifica daca MainViewModel si DbContextFactory sunt disponibile (nu e design-time)
                // Accesam _dbContextFactory prin MainViewModel (acum este public)
                if (_mainViewModel == null || _mainViewModel._dbContextFactory == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real menu data.");
                    // Poti adauga date mock aici pentru design-time (daca nu le-ai adaugat deja in constructorul fara parametri)
                    return;
                }


                using (var context = _mainViewModel._dbContextFactory.CreateDbContext())
                {
                    // Apeleaza procedura stocata GetFullMenuDetails
                    // Mapam rezultatele la o lista de obiecte DisplayMenuItem
                    var menuItemsData = await context.Set<DisplayMenuItem>() // Folosim Set<DisplayMenuItem>() pentru a mapa la un tipo fara cheie
                                                   .FromSqlRaw("EXEC GetFullMenuDetails") // Apelam procedura stocata bruta
                                                   .ToListAsync(); // Executa query-ul si aduce rezultatele

                    // Organizeaza datele pe categorii
                    var categoriesWithItems = new List<CategoryDisplayWrapper>(); // Folosim List temporar

                    // Gruparea se face acum in C# pe baza CategoryId si CategoryName returnate de SP
                    // Folosim o copie a listei pentru a evita modificarea colectiei in timpul iterarii
                    var groupedItems = menuItemsData.GroupBy(item => new { item.CategoryId, item.CategoryName }); // Grupam dupa CategoryId si CategoryName

                    foreach (var group in groupedItems)
                    {
                        // Cauta wrapper-ul categoriei existente sau creeaza unul nou
                        var categoryWrapper = categoriesWithItems.FirstOrDefault(cw => cw.Category.Id == group.Key.CategoryId);
                        if (categoryWrapper == null)
                        {
                            // Creeaza un wrapper pentru fiecare categorie
                            // Asiguram ca Category este initializat corect
                            categoryWrapper = new CategoryDisplayWrapper(new Category { Id = group.Key.CategoryId, Name = group.Key.CategoryName }); // Folosim CategoryName din cheia grupului
                            categoriesWithItems.Add(categoryWrapper);
                        }

                        // Adauga itemii (Dish/MenuItem) din grupul curent la wrapper-ul categoriei
                        // Ordoneaza itemii in cadrul categoriei
                        var orderedItems = group.OrderBy(item => item.ItemName).ToList();
                        foreach (var item in orderedItems)
                        {
                            categoryWrapper.DisplayItems.Add(item); // Adauga direct obiectul DisplayMenuItem mapat din SP
                        }
                    }

                    // Sorteaza categoriile (optional)
                    _fullMenuCategories = categoriesWithItems.OrderBy(c => c.Category.Name).ToList(); // Stocam meniul complet in lista privata

                    // Afisam initial meniul complet (nefiltrat)
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MenuCategories.Clear();
                        foreach (var category in _fullMenuCategories)
                        {
                            MenuCategories.Add(category);
                        }
                        OnPropertyChanged(nameof(MenuCategories)); // Notifica View-ul ca s-a schimbat colectia
                    });


                    SuccessMessage = "Meniul a fost incarcat cu succes.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea meniului: {ex.Message}";
                Debug.WriteLine($"Eroare la incarcarea meniului (SP): {ex.Message}");
            }
        }

        // --- Metoda pentru incarcarea alergenilor ---
        private async Task LoadAllergens()
        {
            try
            {
                // Verifica daca AllergenService este disponibil (nu e design-time)
                if (_allergenService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real allergen data.");
                    return;
                }

                var allergensList = await _allergenService.GetAllAllergensAsync();

                // Adauga un alergen "implicit" pentru optiunea "Fara Alergen Selectat"
                // allergensList.Insert(0, new Allergen { Id = 0, Name = "Fara Alergen Selectat" }); // Optional: Adauga un placeholder

                App.Current.Dispatcher.Invoke(() =>
                {
                    Allergens.Clear();
                    foreach (var allergen in allergensList.OrderBy(a => a.Name))
                    {
                        Allergens.Add(allergen);
                    }
                    // Optional: Selecteaza primul alergen implicit (sau placeholder-ul)
                    // SelectedAllergen = Allergens.FirstOrDefault();
                    OnPropertyChanged(nameof(Allergens)); // Notifica View-ul
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading allergens: {ex.Message}");
                // Poti seta un mesaj de eroare specific pentru alergeni daca vrei
            }
        }


        // --- Metoda pentru filtrarea meniului ---
        private void FilterMenu()
        {
            // Verifica daca meniul complet a fost incarcat
            if (_fullMenuCategories == null || !_fullMenuCategories.Any())
            {
                Debug.WriteLine("Full menu data not loaded yet.");
                return;
            }

            // Creeaza o lista temporara pentru a stoca categoriile filtrate
            var filteredCategories = new List<CategoryDisplayWrapper>();

            // Itereaza prin fiecare categorie din meniul complet
            foreach (var categoryWrapper in _fullMenuCategories)
            {
                // Creeaza un nou wrapper pentru categoria curenta (pentru a nu modifica direct lista completa)
                var newCategoryWrapper = new CategoryDisplayWrapper(categoryWrapper.Category);

                // Filtreaza itemii din categoria curenta
                var filteredItems = categoryWrapper.DisplayItems.Where(item =>
                {
                    // Logica de filtrare
                    bool matchesSearch = true; // Presupunem ca item-ul se potriveste initial

                    if (SelectedSearchType == SearchType.Nume)
                    {
                        // Filtrare dupa nume
                        if (!string.IsNullOrWhiteSpace(SearchText))
                        {
                            // Verifica daca numele item-ului contine textul de cautare (case-insensitive)
                            bool nameContainsText = item.ItemName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;

                            if (IncludeName)
                            {
                                // Cautam itemi al caror nume CONTINE textul de cautare
                                matchesSearch = nameContainsText;
                            }
                            else
                            {
                                // Cautam itemi al caror nume NU CONTINE textul de cautare
                                matchesSearch = !nameContainsText;
                            }
                        }
                        // Daca SearchText este gol, toate itemii se potrivesc la filtrarea dupa nume (indiferent de IncludeName)
                    }
                    else if (SelectedSearchType == SearchType.Alergen)
                    {
                        // Filtrare dupa alergen
                        if (SelectedAllergen != null)
                        {
                            // Verifica daca string-ul de alergeni al item-ului contine numele alergenului selectat
                            bool containsSelectedAllergen = item.AllergensString != null &&
                                                            item.AllergensString.IndexOf(SelectedAllergen.Name, StringComparison.OrdinalIgnoreCase) >= 0;

                            if (IncludeAllergen)
                            {
                                // Cautam itemi care CONTINE alergenul selectat
                                matchesSearch = containsSelectedAllergen;
                            }
                            else
                            {
                                // Cautam itemi care NU CONTINE alergenul selectat
                                matchesSearch = !containsSelectedAllergen;
                            }
                        }
                        // Daca SelectedAllergen este null, toate itemii se potrivesc la filtrarea dupa alergen
                    }

                    return matchesSearch; // Returneaza true daca item-ul se potriveste criteriilor de filtrare
                }).ToList(); // Executa filtrarea si obtine lista de itemi filtrati

                // Adauga itemii filtrati la noul wrapper de categorie
                foreach (var item in filteredItems.OrderBy(item => item.ItemName)) // Pastram ordinea alfabetica in cadrul categoriei
                {
                    newCategoryWrapper.DisplayItems.Add(item);
                }


                // Adauga wrapper-ul de categorie filtrata la lista de categorii filtrate DOAR daca are itemi
                if (newCategoryWrapper.DisplayItems.Any())
                {
                    filteredCategories.Add(newCategoryWrapper);
                }
            }

            // Actualizeaza colectia observabila legata de UI (in UI thread)
            App.Current.Dispatcher.Invoke(() =>
            {
                MenuCategories.Clear();
                foreach (var category in filteredCategories.OrderBy(c => c.Category.Name)) // Pastram ordinea alfabetica a categoriilor
                {
                    MenuCategories.Add(category);
                }
                OnPropertyChanged(nameof(MenuCategories)); // Notifica View-ul ca s-a schimbat colectia
            });
        }
    }

    // Clasa helper pentru a reprezenta un item in cosul de cumparaturi
    // Implementeaza INotifyPropertyChanged pentru a notifica UI-ul la schimbarea cantitatii
    public class CartItem : ViewModelBase // Mosteneste ViewModelBase pentru notificari
    {
        public DisplayMenuItem Item { get; set; } // Referinta la itemul din meniu

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(ItemSubtotal)); // Notifica si subtotalul itemului cand cantitatea se schimba
                // Notifica ViewModel-ul parinte (ClientDashboardViewModel) ca subtotalul cosului s-a schimbat
                // Aceasta necesita o referinta inapoi la ViewModel-ul parinte sau un eveniment
                // Pentru simplitate, ne bazam pe apelul CalculateCartSubtotal() din ExecuteAddToCart/RemoveFromCart
            }
        }

        // Subtotalul pentru acest item (pret unitar * cantitate)
        public decimal ItemSubtotal => Item.ItemPrice * Quantity;

        public CartItem(DisplayMenuItem item, int quantity)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Quantity = quantity;
        }
        // Constructor gol pentru design time (optional)
        public CartItem()
        {
            Item = new DisplayMenuItem(); // Initializeaza Item
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
