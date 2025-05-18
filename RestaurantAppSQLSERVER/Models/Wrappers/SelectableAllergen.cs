using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.ViewModels; // Assuming ViewModelBase is in this namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Wrappers
{
    // Wrapper class for Allergen to add an IsSelected property for UI binding
    public class SelectableAllergen : ViewModelBase // Inheriting ViewModelBase for OnPropertyChanged
    {
        public Allergen Allergen { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                // Optionally notify commands that depend on allergen selection changes
                // This might be needed if Save button state depends on allergen selection
                // System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        // Constructor
        public SelectableAllergen(Allergen allergen, bool isSelected = false)
        {
            Allergen = allergen ?? throw new ArgumentNullException(nameof(allergen));
            _isSelected = isSelected; // Initialize the selected state
        }

        // Override ToString to display the allergen name in UI controls if needed
        public override string ToString()
        {
            return Allergen?.Name ?? string.Empty;
        }
    }
}
