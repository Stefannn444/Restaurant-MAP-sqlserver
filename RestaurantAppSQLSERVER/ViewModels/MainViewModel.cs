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

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(); // Notify the UI that CurrentViewModel has changed
            }
        }

        // To store the logged-in user (optional for now, but will be needed)
        public User LoggedInUser { get; private set; }

        public MainViewModel()
        {
            _dbContextFactory = new DbContextFactory(); // Assumes appsettings.json is in the output directory
            // We'll create IUserService and UserService concrete class in the next step
            // For now, let's assume _userService will be initialized.
             _userService = new UserService(_dbContextFactory);

            // Initially, show the LoginView
             ShowLoginView();
        }

        public void ShowLoginView()
        {
            // CurrentViewModel = new LoginViewModel(_userService, this);
            // We'll create LoginViewModel in a later step. For now, you can comment this out
            // or use a placeholder if you want to run the app.
            CurrentViewModel=new LoginViewModel(_userService, this);
        }

        public void ShowRegisterView()
        {
            // CurrentViewModel = new RegisterViewModel(_userService, this);
            // We'll create RegisterViewModel in a later step.
        }

        public void SetLoggedInUser(User user)
        {
            LoggedInUser = user;
            // After login, you'd navigate to a dashboard or main app view
            // For example: ShowDashboardView();
        }

        public void Logout()
        {
            LoggedInUser = null;
            ShowLoginView();
        }

        // Example of navigating to a view after login (you'll create DashboardViewModel later)
        /*
        public void ShowDashboardView()
        {
            if (LoggedInUser != null)
            {
                // CurrentViewModel = new DashboardViewModel(LoggedInUser, _userService, this /* ... other services ... *///);
        /*
        }
        else
        {
            ShowLoginView(); // If somehow logged out or no user, go back to login
        }
    }
    */
    }
}
