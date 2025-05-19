using RestaurantAppSQLSERVER.Data;
using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        private readonly UserService _userService;
        private readonly DbContextFactory _dbContextFactory;
        private readonly DishService _dishService;
        private readonly CategoryService _categoryService; // Serviciul pentru Categorii
        private readonly AllergenService _allergenService; // Serviciul pentru Alergeni
        private readonly MenuItemService _menuItemService; // Serviciul principal pentru MenuItem (Meniu)
        private readonly OrderService _orderService; // Serviciul pentru Comenzi


        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(); // Notify the UI that CurrentViewModel has changed
            }
        }

        // To store the logged-in user (will be null for guest)
        public User LoggedInUser { get; private set; }

        public MainViewModel()
        {
            _dbContextFactory = new DbContextFactory();
            _userService = new UserService(_dbContextFactory);
            _dishService = new DishService(_dbContextFactory);
            _categoryService = new CategoryService(_dbContextFactory); // Initializeaza CategoryService
            _allergenService = new AllergenService(_dbContextFactory); // Initializeaza AllergenService
            _menuItemService = new MenuItemService(_dbContextFactory); // Initializeaza MenuItemService (Serviciul principal pentru Meniu)
            _orderService = new OrderService(_dbContextFactory); // Initializeaza OrderService


            // Inițial, arată View-ul de Login
            ShowLoginView();
        }

        public void ShowLoginView()
        {
            // La revenirea la login, asigura-te ca userul autentificat este null
            LoggedInUser = null;
            CurrentViewModel = new LoginViewModel(_userService, this);
        }

        public void ShowRegisterView()
        {
            // Va trebui să implementezi RegisterViewModel și să-l instanțiezi aici
            // CurrentViewModel = new RegisterViewModel(_userService, this);
            // Placeholder temporar:
            CurrentViewModel = new RegisterViewModel(_userService, this);
        }

        // Metoda pentru a arata Dashboard-ul Angajatului
        public void ShowEmployeeDashboardView()
        {
            // Verifică dacă utilizatorul autentificat este angajat (optional, dar recomandat)
            if (LoggedInUser != null && LoggedInUser.Rol == UserRole.Angajat)
            {
                // Initializeaza EmployeeDashboardViewModel cu TOATE serviciile necesare
                CurrentViewModel = new EmployeeDashboardViewModel(_dishService, _categoryService, _allergenService, _menuItemService, _orderService, this /*, other services */);
            }
            else
            {
                // Daca nu este angajat, il redirectionezi inapoi la login sau la dashboard-ul clientului
                ShowLoginView(); // Sau ShowClientDashboardView()
            }
        }

        // Metoda pentru a arata Dashboard-ul Clientului (pentru utilizatori autentificati)
        public void ShowClientDashboardView(User user)
        {
            // Seteaza utilizatorul autentificat
            LoggedInUser = user;
            // Creeaza ClientDashboardViewModel, pasand userul autentificat (NU este invitat)
            CurrentViewModel = new ClientDashboardViewModel(LoggedInUser, _categoryService, _dishService, _menuItemService, this);
        }

        // Metoda pentru a arata Dashboard-ul Clientului (pentru invitati)
        public void ShowGuestClientDashboardView()
        {
            // Seteaza utilizatorul autentificat la null pentru sesiunea de invitat
            LoggedInUser = null;
            // Creeaza ClientDashboardViewModel, pasand null pentru user (ESTE invitat)
            CurrentViewModel = new ClientDashboardViewModel(null, _categoryService, _dishService, _menuItemService, this);
        }


        public void SetLoggedInUser(User user)
        {
            LoggedInUser = user;
            // După login, navighează către dashboard-ul corespunzător rolului
            if (LoggedInUser != null)
            {
                if (LoggedInUser.Rol == UserRole.Angajat)
                {
                    ShowEmployeeDashboardView();
                }
                else // Rolul este Client
                {
                    ShowClientDashboardView(LoggedInUser); // Navigheaza la dashboard client autentificat
                }
            }
            else
            {
                // Daca userul este null (login esuat sau logout), ramane pe LoginView (gestionat in LoginViewModel)
            }
        }

        public void Logout()
        {
            LoggedInUser = null;
            ShowLoginView();
        }

        // Metoda placeholder pentru confirmare (daca o folosesti in ViewModel)
        /*
        public Task<bool> ConfirmActionAsync(string message)
        {
            // Implementeaza o fereastra de dialog pentru confirmare
            // Pentru simplitate, returnam true direct pentru moment
            return Task.FromResult(true);
        }
        */
    }
}
