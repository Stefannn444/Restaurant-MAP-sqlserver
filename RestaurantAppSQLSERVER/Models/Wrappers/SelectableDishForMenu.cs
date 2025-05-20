using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantAppSQLSERVER.Models.Wrappers
{
    public class SelectableDishForMenu : ViewModelBase
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
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private int _selectedQuantity;
        public int SelectedQuantity
        {
            get => _selectedQuantity;
            set
            {
                _selectedQuantity = Math.Max(0, value);
                OnPropertyChanged(nameof(SelectedQuantity));
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public SelectableDishForMenu(Dish dish, bool isSelected = false, int selectedQuantity = 0)
        {
            Dish = dish ?? throw new ArgumentNullException(nameof(dish));
            _isSelected = isSelected;
            _selectedQuantity = Math.Max(0, selectedQuantity);
        }
        public override string ToString()
        {
            return Dish?.Name ?? string.Empty;
        }
    }
}
