using RestaurantAppSQLSERVER.Data;
using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestaurantAppSQLSERVER.ViewModels;


namespace RestaurantAppSQLSERVER.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        public readonly DbContextFactory _dbContextFactory;
        private readonly IConfiguration _configuration;


        private readonly UserService _userService;
        private readonly DishService _dishService;
        private readonly CategoryService _categoryService;
        private readonly AllergenService _allergenService;
        private readonly MenuItemService _menuItemService;
        private readonly OrderService _orderService;


        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }
        public User LoggedInUser { get; private set; }

        public MainViewModel()
        {
            _dbContextFactory = new DbContextFactory();
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();


            _userService = new UserService(_dbContextFactory);
            _dishService = new DishService(_dbContextFactory);
            _categoryService = new CategoryService(_dbContextFactory);
            _allergenService = new AllergenService(_dbContextFactory);
            _menuItemService = new MenuItemService(_dbContextFactory);
            _orderService = new OrderService(_dbContextFactory);
            ShowLoginView();
        }

        public void ShowLoginView()
        {
            LoggedInUser = null;
            CurrentViewModel = new LoginViewModel(_userService, this);
        }
        public void ShowRegisterView()
        {
            CurrentViewModel = new RegisterViewModel(_userService, this);
        }
        public void ShowEmployeeDashboardView()
        {
            if (LoggedInUser != null && LoggedInUser.Rol == UserRole.Angajat)
            {
                 CurrentViewModel = new EmployeeDashboardViewModel(_dishService, _categoryService, _allergenService, _menuItemService, _orderService, this);
            }
            else
            {
                ShowLoginView();
            }
        }
        public void ShowClientDashboardView(User user)
        {
            LoggedInUser = user;
            CurrentViewModel = new ClientDashboardViewModel(LoggedInUser, _categoryService, _dishService, _menuItemService, _orderService, _allergenService, this, _configuration);
        }
        public void ShowGuestClientDashboardView()
        {
            LoggedInUser = null;
            CurrentViewModel = new ClientDashboardViewModel(null, _categoryService, _dishService, _menuItemService, _orderService, _allergenService, this, _configuration);
        }
        public void ShowClientOrdersView(User user)
        {
            if (user == null)
            {
                ShowLoginView();
                return;
            }
            CurrentViewModel = new ClientOrdersViewModel(user, _orderService, this);
        }


        public void SetLoggedInUser(User user)
        {
            LoggedInUser = user;
            if (LoggedInUser != null)
            {
                if (LoggedInUser.Rol == UserRole.Angajat)
                {
                     ShowEmployeeDashboardView();
                }
                else
                {
                    ShowClientDashboardView(LoggedInUser);
                }
            }
            else
            {
            }
        }

        public void Logout()
        {
            LoggedInUser = null;
            ShowLoginView();
        }
    }
}
