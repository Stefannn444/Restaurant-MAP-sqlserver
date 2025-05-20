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
    public class RegisterViewModel : ViewModelBase
    {
        private string _nume;
        public string Nume
        {
            get => _nume;
            set
            {
                _nume = value;
                OnPropertyChanged(nameof(Nume));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        private string _prenume;
        public string Prenume
        {
            get => _prenume;
            set
            {
                _prenume = value;
                OnPropertyChanged(nameof(Prenume));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        private string _nrTel;
        public string Nr_tel
        {
            get => _nrTel;
            set
            {
                _nrTel = value;
                OnPropertyChanged(nameof(Nr_tel));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        private string _adresa;
        public string Adresa
        {
            get => _adresa;
            set
            {
                _adresa = value;
                OnPropertyChanged(nameof(Adresa));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        private string _parola;
        public string Parola
        {
            get => _parola;
            set
            {
                _parola = value;
                OnPropertyChanged(nameof(Parola));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        private string _confirmareParola;
        public string ConfirmareParola
        {
            get => _confirmareParola;
            set
            {
                _confirmareParola = value;
                OnPropertyChanged(nameof(ConfirmareParola));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
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
        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        private readonly UserService _userService;
        private readonly MainViewModel _mainViewModel;

        public RegisterViewModel(UserService userService, MainViewModel mainViewModel)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }
        private async void ExecuteRegister(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(Nume) || string.IsNullOrWhiteSpace(Prenume) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Nr_tel) ||
                string.IsNullOrWhiteSpace(Adresa) || string.IsNullOrWhiteSpace(Parola) ||
                string.IsNullOrWhiteSpace(ConfirmareParola))
            {
                ErrorMessage = "Toate câmpurile sunt obligatorii.";
                return;
            }

            if (Parola != ConfirmareParola)
            {
                ErrorMessage = "Parola și confirmarea parolei nu se potrivesc.";
                return;
            }
            var newUser = new User
            {
                Nume = Nume,
                Prenume = Prenume,
                Email = Email,
                Nr_tel = Nr_tel,
                Adresa = Adresa,
                Parola = Parola,
                Rol = UserRole.Client
            };
            bool registrationSuccess = await _userService.RegisterUserAsync(newUser);

            if (registrationSuccess)
            {
                SuccessMessage = "Înregistrare reușită! Vă puteți autentifica acum.";
            }
            else
            {
                ErrorMessage = "Înregistrarea a eșuat. Email-ul ar putea fi deja folosit.";
            }
        }
        private bool CanExecuteRegister(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Nume) && !string.IsNullOrWhiteSpace(Prenume) &&
                   !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Nr_tel) &&
                   !string.IsNullOrWhiteSpace(Adresa) && !string.IsNullOrWhiteSpace(Parola) &&
                   !string.IsNullOrWhiteSpace(ConfirmareParola) &&
                   Parola == ConfirmareParola;
        }
        private void ExecuteNavigateToLogin(object parameter)
        {
            _mainViewModel.ShowLoginView();
        }
    }
}
