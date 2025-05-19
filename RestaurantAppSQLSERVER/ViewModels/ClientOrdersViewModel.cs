using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input; // Pentru ICommand
using System.Diagnostics; // Pentru Debug.WriteLine
using System.Windows; // Adaugat pentru App.Current.Dispatcher

namespace RestaurantAppSQLSERVER.ViewModels
{
    // ViewModel pentru afisarea istoricului comenzilor clientului
    public class ClientOrdersViewModel : ViewModelBase
    {
        private readonly OrderService _orderService;
        private readonly MainViewModel _mainViewModel; // Referinta catre MainViewModel pentru navigare (ex: inapoi)
        private readonly User _loggedInUser; // Utilizatorul autentificat

        // Colectie de comenzi pentru afisare
        public ObservableCollection<Order> Orders { get; set; } // Folosim direct entitatea Order

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

        // Command pentru incarcarea comenzilor
        public ICommand LoadOrdersCommand { get; }
        // Command pentru a naviga inapoi la dashboard
        public ICommand NavigateBackCommand { get; }


        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        // Acest constructor este necesar pentru ca designer-ul XAML sa poata instantia ViewModel-ul
        public ClientOrdersViewModel() : this(null, null, null) // Adaugat null pentru User
        {
            Debug.WriteLine("ClientOrdersViewModel created for Design Time.");
            // Poti adauga date mock pentru design time
            Orders = new ObservableCollection<Order>();
            // Adauga date mock pentru comenzi si itemi
            var mockOrder = new Order
            {
                Id = 1,
                OrderCode = "CMD-MOCK-001",
                OrderDate = DateTime.Now.AddDays(-5),
                Status = "Livrata",
                TotalPrice = 45.50m,
                TransportCost = 10.00m,
                DiscountPercentage = 0m, // Suma reducerii
                Subtotal = 35.50m,
                OrderItems = new List<OrderItem>
                 {
                     new OrderItem { Id = 101, ItemType = "Dish", ItemId = 1, Name = "Mock Dish A", Price = 15.00m, Quantity = 2, TotalPrice = 30.00m },
                     new OrderItem { Id = 102, ItemType = "MenuItem", ItemId = 10, Name = "Mock Meniu B", Price = 20.50m, Quantity = 1, TotalPrice = 20.50m }
                 }
            };
            Orders.Add(mockOrder);
        }


        // Constructorul principal - folosit la RULARE
        public ClientOrdersViewModel(User loggedInUser, OrderService orderService, MainViewModel mainViewModel)
        {
            _loggedInUser = loggedInUser ?? throw new ArgumentNullException(nameof(loggedInUser), "User must be logged in to view orders.");
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

            // Initializeaza colectia
            Orders = new ObservableCollection<Order>();

            // Initializeaza command-urile
            // LoadOrdersCommand va incarca comenzile la initializarea ViewModel-ului
            // CORECTAT: Pasam o metoda care accepta un parametru object (chiar daca nu il folosim)
            LoadOrdersCommand = new RelayCommand(async (parameter) => await ExecuteLoadOrders(parameter));
            NavigateBackCommand = new RelayCommand(ExecuteNavigateBack);


            // Incarca comenzile la initializarea ViewModel-ului
            // Ruleaza intr-un Task separat pentru a nu bloca UI-ul
            Task.Run(async () => await ExecuteLoadOrders(null)); // Pasam null ca parametru
        }

        // --- Metode pentru Command-uri ---

        // Metoda de executie pentru LoadOrdersCommand (async)
        // CORECTAT: Metoda accepta acum un parametru object
        private async Task ExecuteLoadOrders(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                // Verifica daca utilizatorul este autentificat (desi constructorul ar trebui sa asigure asta)
                if (_loggedInUser == null)
                {
                    ErrorMessage = "Nu puteti vizualiza comenzile ca invitat. Va rugam sa va autentificati.";
                    return;
                }

                // Apeleaza OrderService pentru a obtine comenzile utilizatorului
                var clientOrders = await _orderService.GetClientOrdersAsync(_loggedInUser.Id);

                // Actualizeaza colectia observabila (in UI thread)
                App.Current.Dispatcher.Invoke(() =>
                {
                    Orders.Clear();
                    foreach (var order in clientOrders)
                    {
                        Orders.Add(order);
                    }
                });

                SuccessMessage = $"Au fost incarcate {Orders.Count} comenzi.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea comenzilor: {ex.Message}";
                Debug.WriteLine($"Load Orders Error: {ex.Message}");
            }
        }

        // Metoda de executie pentru NavigateBackCommand
        private void ExecuteNavigateBack(object parameter)
        {
            // Navigheaza inapoi la dashboard-ul clientului (autentificat)
            _mainViewModel.ShowClientDashboardView(_loggedInUser);
        }

        // TODO: Adauga Command pentru Anulare Comanda (daca implementezi functionalitatea)
        // Aceasta ar implica o noua procedura stocata sau logica EF Core pentru a schimba Status-ul comenzii
        // si posibil a readauga stocul.
        /*
        public ICommand CancelOrderCommand { get; }
        private async void ExecuteCancelOrder(object parameter)
        {
            if (parameter is Order orderToCancel)
            {
                try
                {
                    // TODO: Apeleaza o metoda in OrderService pentru a anula comanda
                    // var result = await _orderService.CancelOrderAsync(orderToCancel.Id);
                    // if (result.IsSuccess) { ... actualizeaza UI ... }
                    SuccessMessage = $"Comanda {orderToCancel.OrderCode} a fost anulata (simulat).";
                    // Reincarca comenzile pentru a reflecta schimbarea
                    await ExecuteLoadOrders(null); // Apeleaza LoadOrdersCommand cu null
                }
                catch (Exception ex)
                {
                     ErrorMessage = $"Eroare la anularea comenzii {orderToCancel.OrderCode}: {ex.Message}";
                }
            }
        }
        */
    }
}
