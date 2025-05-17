using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantAppSQLSERVER.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // Evenimentul care se declanșează atunci când proprietatea CanExecute s-ar putea schimba.
        // UI-ul se abonează la acest eveniment.
        public event EventHandler CanExecuteChanged;

        // Constructor pentru un command care poate fi executat întotdeauna
        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        // Constructor pentru un command cu o condiție de execuție (CanExecute)
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Determină dacă command-ul poate fi executat în starea curentă.
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Execută logica command-ului.
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        // Metodă pentru a declanșa manual evenimentul CanExecuteChanged.
        // Aceasta notifică UI-ul să re-evalueze starea command-ului.
        public void RaiseCanExecuteChanged()
        {
            // Verifică dacă există subscriberi și declanșează evenimentul.
            // În aplicații WPF, CommandManager.InvalidateRequerySuggested() este adesea folosit
            // pentru a cere CommandManager să re-evalueze CanExecute pentru toate command-urile
            // legate de CommandManager.RequerySuggested. Totuși, o implementare directă
            // a evenimentului ca aici este de asemenea validă, mai ales dacă vrei control granular.
            // Asigură-te că acest apel se face pe thread-ul UI dacă este apelat de pe un alt thread.
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
