using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace RestaurantAppSQLSERVER.ViewModels
{
    public class OrderEmployeeViewModel : ViewModelBase
    {
        public ObservableCollection<Order> Orders { get; set; }
        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)ViewOrderDetailsCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateOrderStatusCommand).RaiseCanExecuteChanged();
            }
        }
        private Order _currentOrderDetails;
        public Order CurrentOrderDetails
        {
            get => _currentOrderDetails;
            set
            {
                _currentOrderDetails = value;
                OnPropertyChanged(nameof(CurrentOrderDetails));
                SelectedOrderStatus = value?.Status ?? string.Empty;
                IsViewingDetails = value != null;
            }
        }
        private string _selectedOrderStatus;
        public string SelectedOrderStatus
        {
            get => _selectedOrderStatus;
            set
            {
                _selectedOrderStatus = value;
                OnPropertyChanged(nameof(SelectedOrderStatus));
                ((RelayCommand)UpdateOrderStatusCommand).RaiseCanExecuteChanged();
            }
        }
        public ObservableCollection<string> AvailableStatuses { get; set; }
        private bool _isViewingDetails;
        public bool IsViewingDetails
        {
            get => _isViewingDetails;
            set
            {
                _isViewingDetails = value;
                OnPropertyChanged(nameof(IsViewingDetails));
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
        public ICommand LoadOrdersCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }
        public ICommand UpdateOrderStatusCommand { get; }
        public ICommand CancelViewDetailsCommand { get; }


        private readonly OrderService _orderService;
        public OrderEmployeeViewModel() : this(null)
        {
        }
        public OrderEmployeeViewModel(OrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            Orders = new ObservableCollection<Order>();
            AvailableStatuses = new ObservableCollection<string>
            {
                "Inregistrata",
                "Se pregateste",
                "Gata",
                "Livrata",
                "Finalizata",
                "Anulata"
            };
            LoadOrdersCommand = new RelayCommand(async (param) => await ExecuteLoadOrders());
            ViewOrderDetailsCommand = new RelayCommand(ExecuteViewOrderDetails, CanExecuteViewOrderDetails);
            UpdateOrderStatusCommand = new RelayCommand(async (param) => await ExecuteUpdateOrderStatus(), CanExecuteUpdateOrderStatus);
            CancelViewDetailsCommand = new RelayCommand(ExecuteCancelViewDetails);
        }

        private async Task ExecuteLoadOrders()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                if (_orderService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real orders.");
                    return;
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
                CurrentOrderDetails = SelectedOrder;
                IsViewingDetails = true;
            }
        }
        private bool CanExecuteViewOrderDetails(object parameter)
        {
            return SelectedOrder != null;
        }


        private async Task ExecuteUpdateOrderStatus()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (CurrentOrderDetails != null && !string.IsNullOrWhiteSpace(SelectedOrderStatus) && CurrentOrderDetails.Status != SelectedOrderStatus)
            {
                try
                {
                    if (_orderService == null)
                    {
                        Debug.WriteLine("Running in design-time context, cannot update real data.");
                        SuccessMessage = $"Starea comenzii {CurrentOrderDetails.OrderCode} a fost actualizata (simulat).";
                        return;
                    }
                    CurrentOrderDetails.Status = SelectedOrderStatus;
                    await _orderService.UpdateOrderAsync(CurrentOrderDetails);
                    var orderInList = Orders.FirstOrDefault(o => o.Id == CurrentOrderDetails.Id);
                    if (orderInList != null)
                    {
                        orderInList.Status = CurrentOrderDetails.Status;
                        await ExecuteLoadOrders();
                    }


                    SuccessMessage = $"Starea comenzii {CurrentOrderDetails.OrderCode} a fost actualizata cu succes.";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Eroare la actualizarea starii comenzii: {ex.Message}";
                }
            }
        }
        private bool CanExecuteUpdateOrderStatus(object parameter)
        {
            return CurrentOrderDetails != null && !string.IsNullOrWhiteSpace(SelectedOrderStatus) && CurrentOrderDetails.Status != SelectedOrderStatus;
        }

        private void ExecuteCancelViewDetails(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            CurrentOrderDetails = null;
            IsViewingDetails = false;
            SelectedOrder = null;
        }
    }
}
