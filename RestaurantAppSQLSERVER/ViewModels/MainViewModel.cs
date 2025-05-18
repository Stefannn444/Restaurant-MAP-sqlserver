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
        private readonly CategoryService _categoryService; // Adauga serviciul pentru Categorii
        private readonly AllergenService _allergenService; // Adauga serviciul pentru Alergeni

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(); // Notify the UI that CurrentViewModel has changed
            }
        }

        // To store the logged-in user
        public User LoggedInUser { get; private set; }

        public MainViewModel()
        {
            _dbContextFactory = new DbContextFactory();
            _userService = new UserService(_dbContextFactory);
            _dishService = new DishService(_dbContextFactory);
            _categoryService = new CategoryService(_dbContextFactory); // Initializeaza CategoryService
            _allergenService = new AllergenService(_dbContextFactory); // Initializeaza AllergenService

            // Inițial, arată View-ul de Login
            ShowLoginView();
        }

        public void ShowLoginView()
        {
            CurrentViewModel = new LoginViewModel(_userService, this);
        }

        public void ShowRegisterView()
        {
            CurrentViewModel = new RegisterViewModel(_userService, this);
        }

        // Metoda pentru a arata Dashboard-ul Angajatului
        public void ShowEmployeeDashboardView()
        {
            // Verifică dacă utilizatorul autentificat este angajat (optional, dar recomandat)
            if (LoggedInUser != null && LoggedInUser.Rol == UserRole.Angajat)
            {
                // Initializeaza EmployeeDashboardViewModel cu TOATE serviciile necesare
                CurrentViewModel = new EmployeeDashboardViewModel(_dishService, _categoryService, _allergenService, this /*, other services */);
            }
            else
            {
                // Daca nu este angajat, il redirectionezi inapoi la login sau la dashboard-ul clientului
                ShowLoginView(); // Sau ShowClientDashboardView()
            }
        }

        // Metoda pentru a arata Dashboard-ul Clientului (va trebui implementata)
        public void ShowClientDashboardView()
        {
            // Implementeaza ClientDashboardViewModel si View
            // CurrentViewModel = new ClientDashboardViewModel(LoggedInUser, _userService, this /*, other services */);
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
                else // Rolul este Client sau altceva
                {
                    ShowClientDashboardView(); // Navigheaza la dashboard client
                }
            }
            else
            {
                // Daca userul este null (login esuat), ramane pe LoginView (gestionat in LoginViewModel)
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
