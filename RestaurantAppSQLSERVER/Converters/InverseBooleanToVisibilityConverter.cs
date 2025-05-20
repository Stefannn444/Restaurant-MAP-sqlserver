using RestaurantAppSQLSERVER.Converters;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestaurantAppSQLSERVER.Converters 
{
    
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        private BooleanToVisibilityConverter _converter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = _converter.Convert(value, targetType, parameter, culture);

            if (result is Visibility visibility)
            {
                if (visibility == Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
                else 
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
          
            throw new NotImplementedException();
        }
    }
}
