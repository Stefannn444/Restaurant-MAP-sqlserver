using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantAppSQLSERVER.ViewModels
{
    public class EmployeeDashboardViewModel : ViewModelBase
    {
        // Proprietate pentru ViewModel-ul sectiunii CRUD curente afisate
        private ViewModelBase _currentCrudViewModel;
        public ViewModelBase CurrentCrudViewModel
        {
            get => _currentCrudViewModel;
            set
            {
                _currentCrudViewModel = value;
                OnPropertyChanged(); // Notifica UI-ul ca s-a schimbat ViewModel-ul curent
            }
        }

        // Command-uri pentru navigarea intre sectiunile CRUD
        public ICommand ShowDishesCrudCommand { get; }
        public ICommand ShowCategoriesCrudCommand { get; }
        public ICommand ShowAllergensCrudCommand { get; }
        // Public ICommand ShowOrdersCommand { get; } // Adauga pentru comenzi
        // Public ICommand ShowMenuItemsCrudCommand { get; } // Adauga pentru meniuri

        // Serviciile necesare (injectate)
        private readonly DishService _dishService;
        private readonly CategoryService _categoryService; // Adauga serviciul pentru Categorii
        private readonly AllergenService _allergenService; // Adauga serviciul pentru Alergeni
        private readonly MainViewModel _mainViewModel; // Referință către MainViewModel pentru navigare (ex: Logout)

        // Servicii pentru alte entitati (vor fi injectate pe masura ce le implementezi)
        // private readonly OrderService _orderService;
        // private readonly MenuItemService _menuItemService;


        public EmployeeDashboardViewModel(DishService dishService, CategoryService categoryService, AllergenService allergenService, MainViewModel mainViewModel /*, other services */)
        {
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService)); // Injecteaza CategoryService
            _allergenService = allergenService ?? throw new ArgumentNullException(nameof(allergenService)); // Injecteaza AllergenService
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

            // Initializeaza command-urile de navigare
            ShowDishesCrudCommand = new RelayCommand(ExecuteShowDishesCrud);
            ShowCategoriesCrudCommand = new RelayCommand(ExecuteShowCategoriesCrud);
            ShowAllergensCrudCommand = new RelayCommand(ExecuteShowAllergensCrud);
            // ShowOrdersCommand = new RelayCommand(ExecuteShowOrders);
            // ShowMenuItemsCrudCommand = new RelayCommand(ExecuteShowMenuItemsCrud);

            // Seteaza ViewModel-ul initial afisat (ex: sectiunea Preparate)
            ExecuteShowDishesCrud(null); // Afiseaza sectiunea Dish la pornire
        }

        // --- Metode pentru Command-urile de Navigare ---

        private void ExecuteShowDishesCrud(object parameter)
        {
            // Creeaza si seteaza DishCrudViewModel ca ViewModel curent al sectiunii
            // Va trebui sa creezi DishCrudViewModel separat, similar cu Category/Allergen
            // CurrentCrudViewModel = new DishCrudViewModel(_dishService); // Exemplu
            // Placeholder temporar pana creezi DishCrudViewModel
            //CurrentCrudViewModel = new PlaceholderViewModel("Gestiune Preparate (CRUD)");
        }

        private void ExecuteShowCategoriesCrud(object parameter)
        {
            // Creeaza si seteaza CategoryCrudViewModel ca ViewModel curent al sectiunii
            CurrentCrudViewModel = new CategoryCrudViewModel(_categoryService);
            // Optional: Incarca datele automat la afisarea sectiunii
            // Task.Run(async () => await ((CategoryCrudViewModel)CurrentCrudViewModel).ExecuteLoadCategories());
        }

        private void ExecuteShowAllergensCrud(object parameter)
        {
            // Creeaza si seteaza AllergenCrudViewModel ca ViewModel curent al sectiunii
            CurrentCrudViewModel = new AllergenCrudViewModel(_allergenService);
            // Optional: Incarca datele automat la afisarea sectiunii
            // Task.Run(async () => await ((AllergenCrudViewModel)CurrentCrudViewModel).ExecuteLoadAllergens());
        }

        /*
        private void ExecuteShowOrders(object parameter)
        {
             // Creeaza si seteaza OrderViewModel ca ViewModel curent al sectiunii
             // CurrentCrudViewModel = new OrderViewModel(_orderService);
        }

        private void ExecuteShowMenuItemsCrud(object parameter)
        {
             // Creeaza si seteaza MenuItemCrudViewModel ca ViewModel curent al sectiunii
             // CurrentCrudViewModel = new MenuItemCrudViewModel(_menuItemService);
        }
        */

        // Placeholder ViewModel (pentru testare inainte de a crea toate ViewModel-urile CRUD)
        // Va trebui sa creezi aceasta clasa simpla
        /*
        public class PlaceholderViewModel : ViewModelBase
        {
            public string Message { get; }
            public PlaceholderViewModel(string message)
            {
                Message = message;
            }
        }
        */
    }
}
