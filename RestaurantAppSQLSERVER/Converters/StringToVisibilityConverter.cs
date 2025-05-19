using System;
using System.Globalization;
using System.Windows; // Necesara pentru Visibility
using System.Windows.Data; // Necesara pentru IValueConverter

namespace RestaurantAppSQLSERVER.Converters
{
    // Converter care transforma un string (sau null) in Visibility
    // Returneaza Visible daca string-ul nu este null sau gol, altfel returneaza Collapsed.
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Verifica daca valoarea este un string si nu este null sau goala
            if (value is string s && !string.IsNullOrEmpty(s))
            {
                // String-ul are continut, returneaza Visible
                return Visibility.Visible;
            }
            else
            {
                // String-ul este null sau gol, returneaza Collapsed
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Convertirea inversa nu este necesara pentru acest scenariu
            throw new NotImplementedException();
        }
    }
}
