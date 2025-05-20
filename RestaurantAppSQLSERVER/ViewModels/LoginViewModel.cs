using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;

namespace RestaurantAppSQLSERVER.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
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
        public ICommand LoginCommand { get; }
        public ICommand ShowRegisterCommand { get; }
        public ICommand ContinueAsGuestCommand { get; }


        private readonly UserService _userService;
        private readonly MainViewModel _mainViewModel;
        public LoginViewModel() : this(null, null)
        {
            Debug.WriteLine("LoginViewModel created for Design Time.");
        }
        public LoginViewModel(UserService userService, MainViewModel mainViewModel)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            ShowRegisterCommand = new RelayCommand(ExecuteShowRegister);
            ContinueAsGuestCommand = new RelayCommand(ExecuteContinueAsGuest);
        }
        private async void ExecuteLogin(object parameter)
        {
            ErrorMessage = string.Empty;
            try
            {
                var user = await _userService.LoginAsync(Email, Password);

                if (user != null)
                {
                    _mainViewModel.SetLoggedInUser(user);
                }
                else
                {
                    ErrorMessage = "Autentificare esuata. Verifica email-ul si parola.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"A aparut o eroare la autentificare: {ex.Message}";
                Debug.WriteLine($"Login Error: {ex.Message}");
            }
        }
        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password);
        }
        private void ExecuteShowRegister(object parameter)
        {
            _mainViewModel.ShowRegisterView();
        }
        private void ExecuteContinueAsGuest(object parameter)
        {
            _mainViewModel.ShowGuestClientDashboardView();
        }
    }
}
