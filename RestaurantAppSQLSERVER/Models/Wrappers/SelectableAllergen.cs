using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Wrappers
{
    public class SelectableAllergen : ViewModelBase
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
            }
        }
        public SelectableAllergen(Allergen allergen, bool isSelected = false)
        {
            Allergen = allergen ?? throw new ArgumentNullException(nameof(allergen));
            _isSelected = isSelected;
        }
        public override string ToString()
        {
            return Allergen?.Name ?? string.Empty;
        }
    }
}
