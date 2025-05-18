using RestaurantAppSQLSERVER.Converters;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestaurantAppSQLSERVER.Converters // Sau in folderul Converters daca ai creat unul
{
    // Converter pentru a inversa logica BooleanToVisibilityConverter
    // true -> Collapsed, false -> Visible
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        private BooleanToVisibilityConverter _converter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Folosim converter-ul standard, apoi inversam rezultatul
            var result = _converter.Convert(value, targetType, parameter, culture);

            if (result is Visibility visibility)
            {
                if (visibility == Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
                else // Collapsed or Hidden
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed; // Default in caz de eroare
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Nu este necesara conversia inversa pentru acest caz
            throw new NotImplementedException();
        }
    }
}
