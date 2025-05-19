using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics; // Adaugat pentru Debug.WriteLine (optional)

namespace RestaurantAppSQLSERVER.ViewModels
{
    // ViewModel principal pentru Dashboard-ul Angajatului
    public class EmployeeDashboardViewModel : ViewModelBase // Asigura-te ca este public
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
        public ICommand ShowMenusCrudCommand { get; }
        public ICommand ShowOrdersCommand { get; } // Adauga pentru Comenzi

        // Command pentru Logout
        public ICommand LogoutCommand { get; }


        // Serviciile necesare (injectate)
        private readonly DishService _dishService;
        private readonly CategoryService _categoryService; // Serviciul pentru Categorii
        private readonly AllergenService _allergenService; // Serviciul pentru Alergeni
        private readonly MenuItemService _menuItemService; // Serviciul principal pentru MenuItem (Meniu)
        private readonly OrderService _orderService; // Serviciul pentru Comenzi


        private readonly MainViewModel _mainViewModel; // Referință către MainViewModel pentru navigare (ex: Logout)


        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        // Acest constructor este necesar pentru ca designer-ul XAML sa poata instantia ViewModel-ul
        public EmployeeDashboardViewModel() : this(null, null, null, null, null, null) // Adaugat un null pentru OrderService
        {
            // Poti adauga aici logica specifica design-time, daca este necesar
            Debug.WriteLine("EmployeeDashboardViewModel created for Design Time.");
        }


        // Constructorul principal - folosit la RULARE
        public EmployeeDashboardViewModel(DishService dishService, CategoryService categoryService, AllergenService allergenService, MenuItemService menuItemService, OrderService orderService, MainViewModel mainViewModel /*, other services */) // Injecteaza OrderService
        {
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService)); // Injecteaza CategoryService
            _allergenService = allergenService ?? throw new ArgumentNullException(nameof(allergenService)); // Injecteaza AllergenService
            _menuItemService = menuItemService ?? throw new ArgumentNullException(nameof(menuItemService)); // Injecteaza MenuItemService (Serviciul principal pentru Meniu)
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService)); // Injecteaza OrderService


            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

            // Initializeaza command-urile de navigare
            ShowDishesCrudCommand = new RelayCommand(ExecuteShowDishesCrud);
            ShowCategoriesCrudCommand = new RelayCommand(ExecuteShowCategoriesCrud);
            ShowAllergensCrudCommand = new RelayCommand(ExecuteShowAllergensCrud);
            ShowMenusCrudCommand = new RelayCommand(ExecuteShowMenusCrud);
            ShowOrdersCommand = new RelayCommand(ExecuteShowOrders); // Initializeaza command-ul pentru Comenzi

            // Initializeaza command-ul de Logout
            LogoutCommand = new RelayCommand(ExecuteLogout);


            // Seteaza ViewModel-ul initial afisat (ex: sectiunea Preparate)
            ExecuteShowDishesCrud(null); // Afiseaza sectiunea Dish la pornire
        }

        // --- Metode pentru Command-urile de Navigare ---

        private void ExecuteShowDishesCrud(object parameter)
        {
            // Creeaza si seteaza DishCrudViewModel ca ViewModel curent al sectiunii
            // Verifica daca serviciile sunt null (cazul design-time) inainte de a le injecta
            CurrentCrudViewModel = new DishCrudViewModel(_dishService, _categoryService, _allergenService); // Injecteaza TOATE serviciile necesare
                                                                                                            // Optional: Incarca datele automat la afisarea sectiunii
                                                                                                            // Daca vrei sa incarci automat, asigura-te ca metoda in ViewModel este async void
                                                                                                            // Task.Run(async () => await ((DishCrudViewModel)CurrentCrudViewModel).LoadDishesCommand.Execute(null));
        }

        private void ExecuteShowCategoriesCrud(object parameter)
        {
            // Creeaza si seteaza CategoryCrudViewModel ca ViewModel curent al sectiunii
            // Verifica daca serviciul este null (cazul design-time) inainte de a-l injecta
            CurrentCrudViewModel = new CategoryCrudViewModel(_categoryService);
            // Optional: Incarca datele automat la afisarea sectiunii
            // Task.Run(async () => await ((CategoryCrudViewModel)CurrentCrudViewModel).LoadCategoriesCommand.Execute(null));
        }

        private void ExecuteShowAllergensCrud(object parameter)
        {
            // Creeaza si seteaza AllergenCrudViewModel ca ViewModel curent al sectiunii
            // Verifica daca serviciul este null (cazul design-time) inainte de a-l injecta
            CurrentCrudViewModel = new AllergenCrudViewModel(_allergenService);
            // Optional: Incarca datele automat la afisarea sectiunii
            // Task.Run(async () => await ((AllergenCrudViewModel)CurrentCrudViewModel).LoadAllergensCommand.Execute(null));
        }

        private void ExecuteShowMenusCrud(object parameter)
        {
            // Creeaza si seteaza MenuCrudViewModel ca ViewModel curent al sectiunii
            // Verifica daca serviciile sunt null (cazul design-time) inainte de a le injecta
            CurrentCrudViewModel = new MenuCrudViewModel(_menuItemService, _dishService, _categoryService); // Injecteaza MenuItemService (principal), DishService, CategoryService
                                                                                                            // Optional: Incarca datele automat la afisarea sectiunii
                                                                                                            // Task.Run(async () => await ((MenuCrudViewModel)CurrentCrudViewModel).LoadMenuItemsCommand.Execute(null)); // Apeleaza LoadMenuItemsCommand
        }

        private void ExecuteShowOrders(object parameter)
        {
            // Creeaza si seteaza OrderEmployeeViewModel ca ViewModel curent al sectiunii
            // Verifica daca serviciul este null (cazul design-time) inainte de a-l injecta
            CurrentCrudViewModel = new OrderEmployeeViewModel(_orderService); // Instantiate the renamed ViewModel
                                                                              // Optional: Incarca datele automat la afisarea sectiunii
                                                                              // Task.Run(async () => await ((OrderEmployeeViewModel)CurrentCrudViewModel).LoadOrdersCommand.Execute(null)); // Apeleaza LoadOrdersCommand
        }

        // Metoda pentru Command-ul de Logout
        private void ExecuteLogout(object parameter)
        {
            _mainViewModel.Logout(); // Apeleaza metoda Logout din MainViewModel
        }
    }
}
