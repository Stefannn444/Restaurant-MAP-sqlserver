using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input; // For ICommand and CommandManager
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace RestaurantAppSQLSERVER.ViewModels
{
    // ViewModel pentru gestionarea Comenzilor in dashboard-ul Angajatului (vizualizare si actualizare stare)
    public class OrderEmployeeViewModel : ViewModelBase // Renamed class
    {
        // Colectie pentru lista de comenzi afisate
        public ObservableCollection<Order> Orders { get; set; }

        // Proprietate pentru selectia curenta in DataGrid (lista de comenzi)
        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
                // Actualizeaza starea butoanelor/controalelor legate de selectie
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)ViewOrderDetailsCommand).RaiseCanExecuteChanged(); // Poate fi executat doar daca o comanda este selectata
                ((RelayCommand)UpdateOrderStatusCommand).RaiseCanExecuteChanged(); // Poate fi executat doar daca o comanda este selectata
                                                                                   // Poti seta si CurrentOrderDetails = value; aici daca vrei sa afisezi detalii automat la selectie
            }
        }

        // Proprietate pentru comanda selectata, afisata in sectiunea de detalii
        private Order _currentOrderDetails;
        public Order CurrentOrderDetails
        {
            get => _currentOrderDetails;
            set
            {
                _currentOrderDetails = value;
                OnPropertyChanged(nameof(CurrentOrderDetails));
                // Actualizeaza proprietatile individuale pentru binding in formularul de detalii/editare stare
                SelectedOrderStatus = value?.Status ?? string.Empty;
                // Poti adauga si alte proprietati individuale daca editezi si altele (ex: EstimatedDeliveryTime)

                // Actualizeaza starea UI-ului (ex: afiseaza sectiunea de detalii)
                IsViewingDetails = value != null; // Afiseaza sectiunea de detalii doar daca o comanda este selectata
            }
        }

        // Proprietate pentru binding la ComboBox-ul de selectie a starii comenzii
        private string _selectedOrderStatus;
        public string SelectedOrderStatus
        {
            get => _selectedOrderStatus;
            set
            {
                _selectedOrderStatus = value;
                OnPropertyChanged(nameof(SelectedOrderStatus));
                // Notifica command-ul de actualizare stare cand starea selectata se schimba
                ((RelayCommand)UpdateOrderStatusCommand).RaiseCanExecuteChanged();
            }
        }

        // Lista de stari posibile pentru comenzi (pentru ComboBox)
        public ObservableCollection<string> AvailableStatuses { get; set; }


        // Starea UI-ului (ex: vizualizare lista, vizualizare detalii)
        private bool _isViewingDetails;
        public bool IsViewingDetails
        {
            get => _isViewingDetails;
            set
            {
                _isViewingDetails = value;
                OnPropertyChanged(nameof(IsViewingDetails));
                // Notifica CommandManager cand starea UI se schimba
                CommandManager.InvalidateRequerySuggested();
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

        // Command-uri pentru operatiile pe comenzi
        public ICommand LoadOrdersCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; } // Command pentru a afisa detaliile comenzii selectate
        public ICommand UpdateOrderStatusCommand { get; } // Command pentru a actualiza starea comenzii selectate
        public ICommand CancelViewDetailsCommand { get; } // Command pentru a reveni la lista


        private readonly OrderService _orderService;
        // Poti injecta si alte servicii daca ai nevoie de date suplimentare (ex: UserService pentru detalii user)


        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        public OrderEmployeeViewModel() : this(null) // Renamed constructor
        {
            // Poti adauga aici date mock pentru a vedea ceva in designer
            // Orders.Add(new Order { Id = 1, OrderCode = "ORD-001", Status = "Inregistrata", TotalPrice = 50m, OrderDate = DateTime.Now, User = new User { Username = "mockuser" }, OrderItems = new List<OrderItem> { new OrderItem { Name = "Mock Dish", Quantity = 1, Price = 50m } } });
            // AvailableStatuses = new ObservableCollection<string> { "Inregistrata", "Se pregateste", "Gata", "Livrata", "Finalizata", "Anulata" };
        }


        // Constructorul principal - folosit la RULARE
        public OrderEmployeeViewModel(OrderService orderService) // Renamed constructor, injected service
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

            // Initializeaza colectiile
            Orders = new ObservableCollection<Order>();
            AvailableStatuses = new ObservableCollection<string>
            {
                "Inregistrata",
                "Se pregateste",
                "Gata",
                "Livrata",
                "Finalizata",
                "Anulata" // Adauga starile relevante
            };


            // Initializeaza command-urile
            LoadOrdersCommand = new RelayCommand(async (param) => await ExecuteLoadOrders());
            ViewOrderDetailsCommand = new RelayCommand(ExecuteViewOrderDetails, CanExecuteViewOrderDetails);
            UpdateOrderStatusCommand = new RelayCommand(async (param) => await ExecuteUpdateOrderStatus(), CanExecuteUpdateOrderStatus);
            CancelViewDetailsCommand = new RelayCommand(ExecuteCancelViewDetails);


            // Incarca comenzile la initializarea ViewModel-ului (optional)
            // Task.Run(async () => await ExecuteLoadOrders());
        }

        // --- Metode pentru Command-uri ---

        private async Task ExecuteLoadOrders()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                // Verifica daca serviciul este null (cazul design-time)
                if (_orderService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real orders.");
                    // Poti adauga date mock aici pentru design-time
                    // Orders.Clear();
                    // Orders.Add(new Order { Id = 1, OrderCode = "ORD-001", Status = "Inregistrata", TotalPrice = 50m, OrderDate = DateTime.Now, User = new User { Username = "mockuser" }, OrderItems = new List<OrderItem> { new OrderItem { Name = "Mock Dish", Quantity = 1, Price = 50m } } });
                    return; // Iesi din metoda daca suntem in design-time
                }

                var ordersList = await _orderService.GetAllOrdersAsync();
                Orders.Clear();
                foreach (var order in ordersList)
                {
                    Orders.Add(order);
                }
                SuccessMessage = $"Au fost incarcate {Orders.Count} comenzi.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea comenzilor: {ex.Message}";
            }
        }

        private void ExecuteViewOrderDetails(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedOrder != null)
            {
                // Seteaza comanda selectata ca fiind comanda curenta pentru detalii
                // Nota: GetAllOrdersAsync ar trebui sa fi incarcat deja OrderItems.
                // Daca nu, ar trebui sa apelezi _orderService.GetOrderByIdAsync(SelectedOrder.Id) aici
                // pentru a incarca detaliile complete, inclusiv OrderItems.
                CurrentOrderDetails = SelectedOrder;
                IsViewingDetails = true; // Trece in modul vizualizare detalii
            }
        }

        // Determina daca butonul View Details poate fi executat
        private bool CanExecuteViewOrderDetails(object parameter)
        {
            // Button is active only if an order is selected in the DataGrid
            return SelectedOrder != null;
        }


        private async Task ExecuteUpdateOrderStatus()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            // Verifica daca o comanda este selectata si o stare noua este selectata
            if (CurrentOrderDetails != null && !string.IsNullOrWhiteSpace(SelectedOrderStatus) && CurrentOrderDetails.Status != SelectedOrderStatus)
            {
                try
                {
                    // Verifica daca serviciul este null (cazul design-time)
                    if (_orderService == null)
                    {
                        Debug.WriteLine("Running in design-time context, cannot update real data.");
                        // Simuleaza actualizarea starii in obiectul mock
                        // CurrentOrderDetails.Status = SelectedOrderStatus;
                        // Optional: Gaseste obiectul in colectia principala si actualizeaza-l si acolo
                        // var orderInList = Orders.FirstOrDefault(o => o.Id == CurrentOrderDetails.Id);
                        // if (orderInList != null) orderInList.Status = SelectedOrderStatus;
                        SuccessMessage = $"Starea comenzii {CurrentOrderDetails.OrderCode} a fost actualizata (simulat).";
                        return;
                    }

                    // Actualizeaza starea comenzii curente
                    CurrentOrderDetails.Status = SelectedOrderStatus;

                    // Apeleaza serviciul para a salva modificarile in baza de date
                    await _orderService.UpdateOrderAsync(CurrentOrderDetails);

                    // Optional: Actualizeaza obiectul in colectia principala pentru a reflecta schimbarea in DataGrid
                    var orderInList = Orders.FirstOrDefault(o => o.Id == CurrentOrderDetails.Id);
                    if (orderInList != null)
                    {
                        orderInList.Status = CurrentOrderDetails.Status; // Actualizeaza starea in colectia principala
                        // Notifica UI-ul ca proprietatea Status s-a schimbat pe obiectul din colectie
                        // Acest lucru ar putea necesita ca entitatea Order sa implementeze INotifyPropertyChanged
                        // sau sa reincarci lista completa.
                        // O solutie simpla este sa reincarci lista completa:
                        await ExecuteLoadOrders(); // Reincarca lista completa dupa actualizare
                    }


                    SuccessMessage = $"Starea comenzii {CurrentOrderDetails.OrderCode} a fost actualizata cu succes.";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Eroare la actualizarea starii comenzii: {ex.Message}";
                }
            }
        }

        // Determina daca butonul Update Status poate fi executat
        private bool CanExecuteUpdateOrderStatus(object parameter)
        {
            // Button is active only if:
            // 1. We are viewing order details (CurrentOrderDetails != null)
            // 2. A status is selected in the ComboBox (!string.IsNullOrWhiteSpace(SelectedOrderStatus))
            // 3. The selected status is different from the current status (optional, but good practice)
            return CurrentOrderDetails != null && !string.IsNullOrWhiteSpace(SelectedOrderStatus) && CurrentOrderDetails.Status != SelectedOrderStatus;
        }

        private void ExecuteCancelViewDetails(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            CurrentOrderDetails = null; // Curata detaliile comenzii
            IsViewingDetails = false; // Revine la vizualizarea listei
            SelectedOrder = null; // Deselecteaza comanda in lista
        }
    }
}
