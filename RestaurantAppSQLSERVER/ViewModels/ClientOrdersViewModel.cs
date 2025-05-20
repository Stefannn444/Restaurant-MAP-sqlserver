using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input; // For ICommand and CommandManager
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows; // Adaugat pentru App.Current.Dispatcher

namespace RestaurantAppSQLSERVER.ViewModels
{
    // ViewModel pentru istoricul comenzilor clientului
    public class ClientOrdersViewModel : ViewModelBase
    {
        // Colectie pentru lista de comenzi afisate
        public ObservableCollection<Order> ClientOrders { get; set; }

        // Proprietate pentru selectia curenta in DataGrid (lista de comenzi)
        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
                // Actualizeaza starea butoanelor/controalelor legate de selectie (inclusiv butonul Anuleaza)
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)CancelOrderCommand)?.RaiseCanExecuteChanged(); // Notifica butonul Anuleaza
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

        // Command-uri
        public ICommand LoadClientOrdersCommand { get; }
        public ICommand CancelOrderCommand { get; } // Command pentru anularea comenzii
        // NOU: Command pentru a naviga inapoi la dashboard-ul clientului
        public ICommand NavigateBackToDashboardCommand { get; }


        private readonly OrderService _orderService;
        private readonly User _loggedInUser; // Referinta la utilizatorul autentificat
        private readonly MainViewModel _mainViewModel; // Referinta catre MainViewModel pentru navigare (ex: inapoi la dashboard)


        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        public ClientOrdersViewModel() : this(null, null, null) // Adaugat null-uri pentru User si MainViewModel
        {
            Debug.WriteLine("ClientOrdersViewModel created for Design Time.");
            // Poti adauga date mock aici pentru a vedea ceva in designer
            // ClientOrders.Add(new Order { Id = 1, OrderCode = "CMD-001", Status = "Inregistrata", TotalPrice = 100m, OrderDate = DateTime.Now, OrderItems = new List<OrderItem> { new OrderItem { Name = "Mock Dish", Quantity = 1, Price = 100m } } });
            // ClientOrders.Add(new Order { Id = 2, OrderCode = "CMD-002", Status = "Gata", TotalPrice = 200m, OrderDate = DateTime.Now.AddDays(-1), OrderItems = new List<OrderItem> { new OrderItem { Name = "Mock Meniu", Quantity = 1, Price = 200m } } });
        }


        // Constructorul principal - folosit la RULARE
        // Primeste utilizatorul autentificat si OrderService
        public ClientOrdersViewModel(User loggedInUser, OrderService orderService, MainViewModel mainViewModel)
        {
            _loggedInUser = loggedInUser ?? throw new ArgumentNullException(nameof(loggedInUser));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));


            // Initializeaza colectia
            ClientOrders = new ObservableCollection<Order>();

            // Initializeaza command-urile
            LoadClientOrdersCommand = new RelayCommand(async (param) => await ExecuteLoadClientOrders());
            CancelOrderCommand = new RelayCommand(async (param) => await ExecuteCancelOrder(), CanExecuteCancelOrder); // Initializeaza command-ul Anuleaza
            // NOU: Initializeaza command-ul de navigare inapoi
            NavigateBackToDashboardCommand = new RelayCommand(ExecuteNavigateBackToDashboard);


            // Incarca comenzile utilizatorului la initializarea ViewModel-ului
            // Apelam direct metoda async, nu o in Task.Run, ca sa ne asiguram ca se incarca la pornire
            // Daca vrei neaparat sa ruleze in background, va trebui sa folosesti Dispatcher.Invoke in interior
            Task.Run(async () => await ExecuteLoadClientOrders());
        }

        // --- Metode pentru Command-uri ---

        private async Task ExecuteLoadClientOrders()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                // Verifica daca serviciul este null (cazul design-time)
                if (_orderService == null || _loggedInUser == null)
                {
                    Debug.WriteLine("Running in design-time context or user not logged in, cannot load real orders.");
                    // Poti adauga date mock aici pentru design-time
                    // ClientOrders.Clear();
                    // ClientOrders.Add(new Order { Id = 1, OrderCode = "CMD-001", Status = "Inregistrata", TotalPrice = 100m, OrderDate = DateTime.Now, OrderItems = new List<OrderItem> { new OrderItem { Name = "Mock Dish", Quantity = 1, Price = 100m } } });
                    // ClientOrders.Add(new Order { Id = 2, OrderCode = "CMD-002", Status = "Gata", TotalPrice = 200m, OrderDate = DateTime.Now.AddDays(-1), OrderItems = new List<OrderItem> { new OrderItem { Name = "Mock Meniu", Quantity = 1, Price = 200m } } });
                    return; // Iesi din metoda daca suntem in design-time sau userul nu e logat
                }

                var ordersList = await _orderService.GetClientOrdersAsync(_loggedInUser.Id);

                // Actualizeaza colectia pe thread-ul UI folosind Dispatcher.Invoke
                App.Current.Dispatcher.Invoke(() =>
                {
                    ClientOrders.Clear();
                    foreach (var order in ordersList)
                    {
                        ClientOrders.Add(order);
                    }
                });

                SuccessMessage = $"Au fost incarcate {ClientOrders.Count} comenzi.";
            }
            catch (Exception ex)
            {
                // Seteaza mesajul de eroare pe thread-ul UI
                App.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"Eroare la incarcarea comenzilor: {ex.Message}";
                });
                Debug.WriteLine($"Eroare la incarcarea comenzilor: {ex.Message}");
            }
        }

        // Logica pentru command-ul de anulare a comenzii
        private async Task ExecuteCancelOrder()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            // Verificam din nou conditiile (desi CanExecute ar trebui sa le gestioneze)
            if (SelectedOrder == null)
            {
                ErrorMessage = "Selectati o comanda pentru a o anula.";
                return;
            }

            // Conditia de stare este verificata si in CanExecute, dar o reconfirmam aici
            if (SelectedOrder.Status != "Inregistrata"&&SelectedOrder.Status!="0")
            {
                ErrorMessage = "Comanda poate fi anulata doar daca este in starea 'Inregistrata'.";
                return;
            }

            // Optional: Cere confirmarea utilizatorului
            // bool confirm = await _mainViewModel.ConfirmActionAsync($"Sunteti sigur ca doriti sa anulati comanda '{SelectedOrder.OrderCode}'?");
            // if (!confirm) return;


            try
            {
                // Apeleaza metoda din OrderService pentru a anula comanda
                var result = await _orderService.CancelOrderAsync(SelectedOrder.Id, _loggedInUser.Id);

                if (result.IsSuccess)
                {
                    SuccessMessage = result.Message;
                    // Reincarca lista de comenzi pentru a reflecta schimbarea starii
                    await ExecuteLoadClientOrders();
                }
                else
                {
                    // Afiseaza mesajul de eroare returnat de serviciu
                    ErrorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                // Gestioneaza erorile neasteptate
                ErrorMessage = $"A aparut o eroare la anularea comenzii: {ex.Message}";
                Debug.WriteLine($"Cancel Order Error: {ex.Message}");
            }
        }

        // Determina daca butonul Anuleaza poate fi executat
        private bool CanExecuteCancelOrder(object parameter)
        {
            // Butonul Anuleaza este activ doar daca:
            // 1. O comanda este selectata (SelectedOrder != null)
            // 2. Starea comenzii selectate este "Inregistrata"
            return SelectedOrder != null && (SelectedOrder.Status == "Inregistrata"||SelectedOrder.Status=="0");
        }

        // NOU: Logica pentru command-ul de navigare inapoi la dashboard-ul clientului
        private void ExecuteNavigateBackToDashboard(object parameter)
        {
            // Apeleaza metoda din MainViewModel pentru a schimba view-ul inapoi la dashboard-ul clientului
            // Pasam utilizatorul autentificat pentru a mentine sesiunea
            _mainViewModel.ShowClientDashboardView(_loggedInUser);
        }

    }
}
