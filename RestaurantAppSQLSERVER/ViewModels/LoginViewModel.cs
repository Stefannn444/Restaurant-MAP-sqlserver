using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
                // Notifică command-ul de login că starea CanExecute s-ar putea schimba
                // Necesită casting la RelayCommand pentru a accesa metoda RaiseCanExecuteChanged
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        private string _password;
        // Notă: Binding-ul la PasswordBox.Password este problematic în WPF din motive de securitate.
        // O soluție standard implică Attached Properties sau code-behind.
        // Pentru simplitate în acest proiect școlar, vom folosi un string,
        // dar reține că într-o aplicație reală ai vrea să gestionezi parolele mai sigur.
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password)); // Numele proprietății este corect aici
                // Notifică command-ul de login că starea CanExecute s-ar putea schimba
                // Necesită casting la RelayCommand pentru a accesa metoda RaiseCanExecuteChanged
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

        // Declaram command-ul ca ICommand, dar il initializam cu RelayCommand
        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        private readonly UserService _userService;
        private readonly MainViewModel _mainViewModel; // Referință către MainViewModel pentru navigare

        public LoginViewModel(UserService userService, MainViewModel mainViewModel)
        {
            // Verificări pentru null pentru a evita ArgumentNullException
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

            // Inițializarea command-urilor
            // LoginCommand este initializat ca RelayCommand, chiar daca proprietatea este de tip ICommand
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            NavigateToRegisterCommand = new RelayCommand(ExecuteNavigateToRegister);

            // Inițial, șterge orice mesaj de eroare
            ErrorMessage = string.Empty;
        }

        // Logica pentru command-ul de Login
        private async void ExecuteLogin(object parameter)
        {
            ErrorMessage = string.Empty; // Șterge mesajul anterior de eroare

            // Validare simplă
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vă rugăm introduceți email-ul și parola.";
                return;
            }

            // Apelează serviciul de utilizatori pentru a încerca autentificarea
            // Metoda LoginAsync este acum disponibilă în UserService
            User loggedInUser = await _userService.LoginAsync(Email, Password);

            if (loggedInUser != null)
            {
                // Autentificare reușită
                _mainViewModel.SetLoggedInUser(loggedInUser);
                // Navighează către dashboard sau alt view principal
                // _mainViewModel.ShowDashboardView(); // Va trebui să implementezi această metodă în MainViewModel
                ErrorMessage = "Autentificare reușită!"; // Mesaj temporar până la implementarea navigării
            }
            else
            {
                // Autentificare eșuată
                ErrorMessage = "Email sau parolă incorectă.";
            }
        }

        // Determină dacă command-ul de Login poate fi executat
        private bool CanExecuteLogin(object parameter)
        {
            // Command-ul poate fi executat doar dacă ambele câmpuri nu sunt goale
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        // Logica pentru command-ul de navigare către Register
        private void ExecuteNavigateToRegister(object parameter)
        {
            _mainViewModel.ShowRegisterView(); // Apelează metoda din MainViewModel pentru a schimba view-ul
        }
    }
}
