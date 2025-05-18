using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.ViewModels
{
    // ViewModel principal pentru Dashboard-ul Angajatului
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
        private readonly CategoryService _categoryService; // Serviciul pentru Categorii
        private readonly AllergenService _allergenService; // Serviciul pentru Alergeni
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
            CurrentCrudViewModel = new DishCrudViewModel(_dishService, _categoryService, _allergenService); // Injecteaza TOATE serviciile necesare
                                                                                                            // Optional: Incarca datele automat la afisarea sectiunii
                                                                                                            // Daca vrei sa incarci automat, asigura-te ca metoda in ViewModel este async void
                                                                                                            // Task.Run(async () => await ((DishCrudViewModel)CurrentCrudViewModel).LoadDishesCommand.Execute(null));
        }

        private void ExecuteShowCategoriesCrud(object parameter)
        {
            // Creeaza si seteaza CategoryCrudViewModel ca ViewModel curent al sectiunii
            CurrentCrudViewModel = new CategoryCrudViewModel(_categoryService);
            // Optional: Incarca datele automat la afisarea sectiunii
            // Task.Run(async () => await ((CategoryCrudViewModel)CurrentCrudViewModel).LoadCategoriesCommand.Execute(null));
        }

        private void ExecuteShowAllergensCrud(object parameter)
        {
            // Creeaza si seteaza AllergenCrudViewModel ca ViewModel curent al sectiunii
            CurrentCrudViewModel = new AllergenCrudViewModel(_allergenService);
            // Optional: Incarca datele automat la afisarea sectiunii
            // Task.Run(async () => await ((AllergenCrudViewModel)CurrentCrudViewModel).LoadAllergensCommand.Execute(null));
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

        // Placeholder ViewModel (nu mai este necesar daca DishCrudViewModel este implementat)
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
