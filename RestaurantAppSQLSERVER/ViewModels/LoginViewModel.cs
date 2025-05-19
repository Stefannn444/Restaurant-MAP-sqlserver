using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input; // For ICommand
using System.Diagnostics; // For Debug.WriteLine

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
                // Trigger CanExecute check for LoginCommand when Email changes
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
                // Trigger CanExecute check for LoginCommand when Password changes
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

        // Command pentru Login
        public ICommand LoginCommand { get; }
        // Command pentru a naviga la pagina de Register (daca o implementezi)
        public ICommand ShowRegisterCommand { get; } // Declarat
        // Noul Command pentru "Continue as Guest"
        public ICommand ContinueAsGuestCommand { get; }


        private readonly UserService _userService;
        private readonly MainViewModel _mainViewModel; // Referinta catre MainViewModel pentru navigare

        // Constructor pentru Design Time (fara parametri)
        public LoginViewModel() : this(null, null)
        {
            Debug.WriteLine("LoginViewModel created for Design Time.");
            // Poti adauga date mock aici pentru a vedea ceva in designer
            // Email = "test@example.com";
            // Password = "password";
            // ErrorMessage = "Acesta este un mesaj de eroare de design time.";
        }


        // Constructorul principal - folosit la RULARE
        public LoginViewModel(UserService userService, MainViewModel mainViewModel)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

            // Initializeaza command-urile
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            // ShowRegisterCommand este acum INITIALIZAT
            ShowRegisterCommand = new RelayCommand(ExecuteShowRegister);
            ContinueAsGuestCommand = new RelayCommand(ExecuteContinueAsGuest); // Initializeaza noul command
        }

        // --- Metode pentru Command-uri ---

        // Metoda de executie pentru LoginCommand (async)
        private async void ExecuteLogin(object parameter)
        {
            ErrorMessage = string.Empty; // Curata mesajele anterioare
            try
            {
                // Apeleaza serviciul de autentificare
                var user = await _userService.LoginAsync(Email, Password);

                if (user != null)
                {
                    // Autentificare reusita
                    // Seteaza utilizatorul autentificat in MainViewModel si navigheaza
                    _mainViewModel.SetLoggedInUser(user);
                }
                else
                {
                    // Autentificare esuata
                    ErrorMessage = "Autentificare esuata. Verifica email-ul si parola.";
                }
            }
            catch (Exception ex)
            {
                // Gestioneaza erorile (ex: probleme de conexiune la baza de date)
                ErrorMessage = $"A aparut o eroare la autentificare: {ex.Message}";
                Debug.WriteLine($"Login Error: {ex.Message}");
            }
        }

        // Metoda CanExecute pentru LoginCommand (activ doar daca email si parola sunt completate)
        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password);
        }

        // Metoda de executie pentru ShowRegisterCommand - DECOMENTATA
        private void ExecuteShowRegister(object parameter)
        {
            _mainViewModel.ShowRegisterView(); // Apeleaza metoda de navigare din MainViewModel
        }


        // Metoda de executie pentru ContinueAsGuestCommand
        private void ExecuteContinueAsGuest(object parameter)
        {
            // Apeleaza metoda din MainViewModel pentru a arata dashboard-ul clientului ca invitat
            _mainViewModel.ShowGuestClientDashboardView();
        }
    }
}
