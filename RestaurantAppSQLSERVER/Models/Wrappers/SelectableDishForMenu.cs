using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.ViewModels; // Assuming ViewModelBase is in this namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input; // Adaugat pentru CommandManager

namespace RestaurantAppSQLSERVER.Models.Wrappers
{
    // Wrapper class for Dish to add IsSelected and SelectedQuantity properties for UI binding in the Menu CRUD
    public class SelectableDishForMenu : ViewModelBase // Inheriting ViewModelBase for OnPropertyChanged
    {
        public Dish Dish { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                // Notifica CommandManager cand selectia se schimba
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private int _selectedQuantity;
        public int SelectedQuantity
        {
            get => _selectedQuantity;
            set
            {
                // Asigura-te ca Quantity nu este negativ
                _selectedQuantity = Math.Max(0, value);
                OnPropertyChanged(nameof(SelectedQuantity));
                // Notifica CommandManager cand cantitatea se schimba
                CommandManager.InvalidateRequerySuggested();
            }
        }


        // Constructor
        public SelectableDishForMenu(Dish dish, bool isSelected = false, int selectedQuantity = 0)
        {
            Dish = dish ?? throw new ArgumentNullException(nameof(dish));
            _isSelected = isSelected; // Initialize the selected state
            _selectedQuantity = Math.Max(0, selectedQuantity); // Initialize the quantity (ensure non-negative)
        }

        // Override ToString to display the dish name in UI controls if needed
        public override string ToString()
        {
            return Dish?.Name ?? string.Empty;
        }
    }
}
