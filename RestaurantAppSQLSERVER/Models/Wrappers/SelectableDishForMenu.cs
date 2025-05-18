using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.ViewModels; // Assuming ViewModelBase is in this namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Wrappers
{
    // Wrapper class for Dish to add an IsSelected property for UI binding in the Menu CRUD
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
                // Optionally notify commands that depend on dish selection changes
                // This might be needed if Save button state depends on selected dishes
                // System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        // Constructor
        public SelectableDishForMenu(Dish dish, bool isSelected = false)
        {
            Dish = dish ?? throw new ArgumentNullException(nameof(dish));
            _isSelected = isSelected; // Initialize the selected state
        }

        // Override ToString to display the dish name in UI controls if needed
        public override string ToString()
        {
            return Dish?.Name ?? string.Empty;
        }
    }
}
