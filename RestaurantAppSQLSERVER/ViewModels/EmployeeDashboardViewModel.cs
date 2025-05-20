using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RestaurantAppSQLSERVER.ViewModels
{
    public class EmployeeDashboardViewModel : ViewModelBase
    {
        private ViewModelBase _currentCrudViewModel;
        public ViewModelBase CurrentCrudViewModel
        {
            get => _currentCrudViewModel;
            set
            {
                _currentCrudViewModel = value;
                OnPropertyChanged();
            }
        }
        public ICommand ShowDishesCrudCommand { get; }
        public ICommand ShowCategoriesCrudCommand { get; }
        public ICommand ShowAllergensCrudCommand { get; }
        public ICommand ShowMenusCrudCommand { get; }
        public ICommand ShowOrdersCommand { get; }
        public ICommand LogoutCommand { get; }
        private readonly DishService _dishService;
        private readonly CategoryService _categoryService;
        private readonly AllergenService _allergenService;
        private readonly MenuItemService _menuItemService;
        private readonly OrderService _orderService;


        private readonly MainViewModel _mainViewModel;
        public EmployeeDashboardViewModel() : this(null, null, null, null, null, null)
        {
            Debug.WriteLine("EmployeeDashboardViewModel created for Design Time.");
        }
        public EmployeeDashboardViewModel(DishService dishService, CategoryService categoryService, AllergenService allergenService, MenuItemService menuItemService, OrderService orderService, MainViewModel mainViewModel)
        {
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _allergenService = allergenService ?? throw new ArgumentNullException(nameof(allergenService));
            _menuItemService = menuItemService ?? throw new ArgumentNullException(nameof(menuItemService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));


            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            ShowDishesCrudCommand = new RelayCommand(ExecuteShowDishesCrud);
            ShowCategoriesCrudCommand = new RelayCommand(ExecuteShowCategoriesCrud);
            ShowAllergensCrudCommand = new RelayCommand(ExecuteShowAllergensCrud);
            ShowMenusCrudCommand = new RelayCommand(ExecuteShowMenusCrud);
            ShowOrdersCommand = new RelayCommand(ExecuteShowOrders);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            ExecuteShowDishesCrud(null);
        }

        private void ExecuteShowDishesCrud(object parameter)
        {
            CurrentCrudViewModel = new DishCrudViewModel(_dishService, _categoryService, _allergenService);
        }

        private void ExecuteShowCategoriesCrud(object parameter)
        {
            CurrentCrudViewModel = new CategoryCrudViewModel(_categoryService);
        }

        private void ExecuteShowAllergensCrud(object parameter)
        {
            CurrentCrudViewModel = new AllergenCrudViewModel(_allergenService);
        }

        private void ExecuteShowMenusCrud(object parameter)
        {
            CurrentCrudViewModel = new MenuCrudViewModel(_menuItemService, _dishService, _categoryService);
        }

        private void ExecuteShowOrders(object parameter)
        {
            CurrentCrudViewModel = new OrderEmployeeViewModel(_orderService);
        }
        private void ExecuteLogout(object parameter)
        {
            _mainViewModel.Logout();
        }
    }
}
