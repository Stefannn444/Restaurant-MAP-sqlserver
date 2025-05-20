using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows;

namespace RestaurantAppSQLSERVER.ViewModels
{
    public class ClientOrdersViewModel : ViewModelBase
    {
        public ObservableCollection<Order> ClientOrders { get; set; }
        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)CancelOrderCommand)?.RaiseCanExecuteChanged();
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
        public ICommand LoadClientOrdersCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand NavigateBackToDashboardCommand { get; }


        private readonly OrderService _orderService;
        private readonly User _loggedInUser;
        private readonly MainViewModel _mainViewModel;
        public ClientOrdersViewModel() : this(null, null, null)
        {
            Debug.WriteLine("ClientOrdersViewModel created for Design Time.");
        }
        public ClientOrdersViewModel(User loggedInUser, OrderService orderService, MainViewModel mainViewModel)
        {
            _loggedInUser = loggedInUser ?? throw new ArgumentNullException(nameof(loggedInUser));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            ClientOrders = new ObservableCollection<Order>();
            LoadClientOrdersCommand = new RelayCommand(async (param) => await ExecuteLoadClientOrders());
            CancelOrderCommand = new RelayCommand(async (param) => await ExecuteCancelOrder(), CanExecuteCancelOrder);
            NavigateBackToDashboardCommand = new RelayCommand(ExecuteNavigateBackToDashboard);
            Task.Run(async () => await ExecuteLoadClientOrders());
        }

        private async Task ExecuteLoadClientOrders()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                if (_orderService == null || _loggedInUser == null)
                {
                    Debug.WriteLine("Running in design-time context or user not logged in, cannot load real orders.");
                    return;
                }

                var ordersList = await _orderService.GetClientOrdersAsync(_loggedInUser.Id);
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
                App.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"Eroare la incarcarea comenzilor: {ex.Message}";
                });
                Debug.WriteLine($"Eroare la incarcarea comenzilor: {ex.Message}");
            }
        }
        private async Task ExecuteCancelOrder()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedOrder == null)
            {
                ErrorMessage = "Selectati o comanda pentru a o anula.";
                return;
            }
            if (SelectedOrder.Status != "Inregistrata"&&SelectedOrder.Status!="0")
            {
                ErrorMessage = "Comanda poate fi anulata doar daca este in starea 'Inregistrata'.";
                return;
            }


            try
            {
                var result = await _orderService.CancelOrderAsync(SelectedOrder.Id, _loggedInUser.Id);

                if (result.IsSuccess)
                {
                    SuccessMessage = result.Message;
                    await ExecuteLoadClientOrders();
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"A aparut o eroare la anularea comenzii: {ex.Message}";
                Debug.WriteLine($"Cancel Order Error: {ex.Message}");
            }
        }
        private bool CanExecuteCancelOrder(object parameter)
        {
            return SelectedOrder != null && (SelectedOrder.Status == "Inregistrata"||SelectedOrder.Status=="0");
        }
        private void ExecuteNavigateBackToDashboard(object parameter)
        {
            _mainViewModel.ShowClientDashboardView(_loggedInUser);
        }

    }
}
