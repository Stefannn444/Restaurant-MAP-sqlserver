using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RestaurantAppSQLSERVER.Converters
{
    // Acest converter transforma o cale de fisier (string) intr-un obiect ImageSource
    // Presupune ca folderul "Photos" este copiat in directorul de output al aplicatiei.
    public class PathToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    // Obtine directorul de baza al aplicatiei (unde se afla executabilul si folderul Photos copiat)
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    // Combina directorul de baza cu folderul "Photos" si calea imaginii
                    string fullPath = Path.Combine(baseDirectory, "Photos", imagePath);

                    // Verifica daca fisierul exista
                    if (File.Exists(fullPath))
                    {
                        // Creeaza un BitmapImage
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(fullPath, UriKind.RelativeOrAbsolute); // Folosim UriKind.RelativeOrAbsolute
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Incarca imaginea imediat
                        bitmapImage.EndInit();
                        return bitmapImage;
                    }
                    else
                    {
                        // Returneaza null sau un placeholder daca fisierul nu exista
                        // Poti returna o imagine default aici daca vrei
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    // Gestioneaza potentialele erori la incarcarea imaginii
                    System.Diagnostics.Debug.WriteLine($"Error loading image from path: {imagePath}. Error: {ex.Message}");
                    return null; // Returneaza null in caz de eroare
                }
            }
            // Returneaza null daca calea este nula sau goala
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Convertirea inversa nu este necesara pentru afisare
            throw new NotImplementedException();
        }
    }
}
