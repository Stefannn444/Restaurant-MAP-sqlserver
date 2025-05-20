using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.Models.Wrappers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Windows;
using System.Text.RegularExpressions;


namespace RestaurantAppSQLSERVER.ViewModels
{
    public class ClientDashboardViewModel : ViewModelBase
    {
        public ObservableCollection<CategoryDisplayWrapper> MenuCategories { get; set; }
        private List<CategoryDisplayWrapper> _fullMenuCategories;
        public ObservableCollection<Allergen> Allergens { get; set; }
        public ObservableCollection<CartItem> ShoppingCart { get; set; }
        private decimal _cartSubtotal;
        public decimal CartSubtotal
        {
            get => _cartSubtotal;
            set
            {
                _cartSubtotal = value;
                OnPropertyChanged(nameof(CartSubtotal));
                CalculateDiscountAndTransport();
                ((RelayCommand)PlaceOrderCommand)?.RaiseCanExecuteChanged();
            }
        }

        private decimal _discountAmount;
        public decimal DiscountAmount
        {
            get => _discountAmount;
            set
            {
                _discountAmount = value;
                OnPropertyChanged(nameof(DiscountAmount));
                CalculateFinalTotals();
            }
        }

        private decimal _transportCost;
        public decimal TransportCost
        {
            get => _transportCost;
            set
            {
                _transportCost = value;
                OnPropertyChanged(nameof(TransportCost));
                CalculateFinalTotals();
            }
        }

        private decimal _cartTotal;
        public decimal CartTotal
        {
            get => _cartTotal;
            set
            {
                _cartTotal = value;
                OnPropertyChanged(nameof(CartTotal));
            }
        }
        private bool _isGuest;
        public bool IsGuest
        {
            get => _isGuest;
            set
            {
                _isGuest = value;
                OnPropertyChanged(nameof(IsGuest));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)ShowClientOrdersCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand)AddToCartCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand)PlaceOrderCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand)ShowLoginCommand)?.RaiseCanExecuteChanged();
            }
        }
        private User _loggedInUser;
        public User LoggedInUser
        {
            get => _loggedInUser;
            set
            {
                _loggedInUser = value;
                OnPropertyChanged(nameof(LoggedInUser));
                IsGuest = (value == null);
                Task.Run(() => CalculateDiscountAndTransport());
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged(nameof(SuccessMessage));
            }
        }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        private SearchType _selectedSearchType;
        public SearchType SelectedSearchType
        {
            get => _selectedSearchType;
            set
            {
                _selectedSearchType = value;
                OnPropertyChanged(nameof(SelectedSearchType));
                SearchText = string.Empty;
                SelectedAllergen = null;
            }
        }

        private Allergen _selectedAllergen;
        public Allergen SelectedAllergen
        {
            get => _selectedAllergen;
            set
            {
                _selectedAllergen = value;
                OnPropertyChanged(nameof(SelectedAllergen));
            }
        }

        private bool _includeAllergen = true;
        public bool IncludeAllergen
        {
            get => _includeAllergen;
            set
            {
                _includeAllergen = value;
                OnPropertyChanged(nameof(IncludeAllergen));
            }
        }
        private bool _includeName = true;
        public bool IncludeName
        {
            get => _includeName;
            set
            {
                _includeName = value;
                OnPropertyChanged(nameof(IncludeName));
            }
        }
        public enum SearchType
        {
            Nume,
            Alergen
        }
        public ICommand ShowClientOrdersCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowLoginCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand PlaceOrderCommand { get; }
        private readonly CategoryService _categoryService;
        private readonly DishService _dishService;
        private readonly MenuItemService _menuItemService;
        private readonly OrderService _orderService;
        private readonly AllergenService _allergenService;
        private readonly MainViewModel _mainViewModel;
        private readonly IConfiguration _configuration;
        public ClientDashboardViewModel() : this(null, null, null, null, null, null, null, null)
        {
            Debug.WriteLine("ClientDashboardViewModel created for Design Time.");
            MenuCategories = new ObservableCollection<CategoryDisplayWrapper>();
            _fullMenuCategories = new List<CategoryDisplayWrapper>();
            ShoppingCart = new ObservableCollection<CartItem>();
            Allergens = new ObservableCollection<Allergen>();

            var mockCategory = new CategoryDisplayWrapper(new Category { Name = "Mock Categorie 1" });
            var mockDish = new DisplayMenuItem { ItemId = 1, ItemName = "Mock Dish 1", ItemPrice = 10m, ItemType = "Dish", QuantityDisplay = "250g", AllergensString = "Gluten, Lactoza" };
            var mockMenuItem = new DisplayMenuItem { ItemId = 10, ItemName = "Mock Meniu 1", ItemPrice = 25m, ItemType = "MenuItem", QuantityDisplay = "Meniu", MenuItemComponentsString = "1x Mock Dish 1; 1x Mock Dish 2", AllergensString = "Gluten, Nuci" };
            mockCategory.DisplayItems.Add(mockDish);
            mockCategory.DisplayItems.Add(mockMenuItem);
            MenuCategories.Add(mockCategory);
            _fullMenuCategories.Add(mockCategory);

            Allergens.Add(new Allergen { Id = 1, Name = "Gluten" });
            Allergens.Add(new Allergen { Id = 2, Name = "Lactoza" });
            Allergens.Add(new Allergen { Id = 3, Name = "Nuci" });
            SelectedSearchType = SearchType.Nume;
            ShoppingCart.Add(new CartItem(mockDish, 2));
            ShoppingCart.Add(new CartItem(mockMenuItem, 1));
            CalculateCartSubtotal();
            DiscountAmount = 2.50m;
            TransportCost = 15.00m;
            CalculateFinalTotals();
        }
        public ClientDashboardViewModel(User loggedInUser, CategoryService categoryService, DishService dishService, MenuItemService menuItemService, OrderService orderService, AllergenService allergenService, MainViewModel mainViewModel, IConfiguration configuration)
        {
            MenuCategories = new ObservableCollection<CategoryDisplayWrapper>();
            _fullMenuCategories = new List<CategoryDisplayWrapper>();
            ShoppingCart = new ObservableCollection<CartItem>();
            Allergens = new ObservableCollection<Allergen>();
            ShowClientOrdersCommand = new RelayCommand(ExecuteShowClientOrders, CanExecuteShowClientOrders);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            ShowLoginCommand = new RelayCommand(ExecuteShowLogin, CanExecuteShowLogin);
            AddToCartCommand = new RelayCommand(ExecuteAddToCart, CanExecuteAddToCart);
            RemoveFromCartCommand = new RelayCommand(ExecuteRemoveFromCart);
            PlaceOrderCommand = new RelayCommand(async (param) => await ExecutePlaceOrder(param), CanExecutePlaceOrder);
            SearchCommand = new RelayCommand(ExecuteSearch);
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _menuItemService = menuItemService ?? throw new ArgumentNullException(nameof(menuItemService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _allergenService = allergenService ?? throw new ArgumentNullException(nameof(allergenService));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(_configuration));
            LoggedInUser = loggedInUser;
            SelectedSearchType = SearchType.Nume;
            Task.Run(async () => await LoadInitialData());
            CalculateCartSubtotal();
        }
        private void ExecuteShowClientOrders(object parameter)
        {
            _mainViewModel.ShowClientOrdersView(LoggedInUser);
        }
        private bool CanExecuteShowClientOrders(object parameter)
        {
            return !IsGuest;
        }


        private void ExecuteLogout(object parameter)
        {
            _mainViewModel.Logout();
        }
        private void ExecuteShowLogin(object parameter)
        {
            _mainViewModel.ShowLoginView();
        }
        private bool CanExecuteShowLogin(object parameter)
        {
            return IsGuest;
        }
        private void ExecuteSearch(object parameter)
        {
            if (parameter is string paramString && paramString == "Reset")
            {
                SearchText = string.Empty;
                SelectedSearchType = SearchType.Nume;
                SelectedAllergen = null;
                IncludeAllergen = true;
                IncludeName = true;
            }

            FilterMenu();
        }
        private void ExecuteAddToCart(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (parameter is DisplayMenuItem item)
            {
                var existingCartItem = ShoppingCart.FirstOrDefault(ci => ci.Item.ItemId == item.ItemId && ci.Item.ItemType == item.ItemType);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity++;
                }
                else
                {
                    ShoppingCart.Add(new CartItem(item, 1));
                }

                CalculateCartSubtotal();
                SuccessMessage = $"{item.ItemName} a fost adaugat in cos.";
            }
        }
        private bool CanExecuteAddToCart(object parameter)
        {
            return !IsGuest;
        }
        private void ExecuteRemoveFromCart(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (parameter is CartItem itemToRemove)
            {
                if (itemToRemove.Quantity > 1)
                {
                    itemToRemove.Quantity--;
                }
                else
                {
                    ShoppingCart.Remove(itemToRemove);
                }

                CalculateCartSubtotal();
                SuccessMessage = $"{itemToRemove.Item.ItemName} a fost eliminat din cos.";
            }
        }
        private async Task ExecutePlaceOrder(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (IsGuest || LoggedInUser == null)
            {
                ErrorMessage = "Trebuie sa fiti autentificat pentru a plasa o comanda.";
                return;
            }

            if (!ShoppingCart.Any())
            {
                ErrorMessage = "Cosul de cumparaturi este gol.";
                return;
            }

            try
            {
                var orderItemsData = ShoppingCart.Select(cartItem =>
                {
                    int quantityToSend;

                    if (cartItem.Item.ItemType == "Dish")
                    {
                        decimal unitGramage = ParseQuantityDisplay(cartItem.Item.QuantityDisplay);
                        quantityToSend = (int)(unitGramage * cartItem.Quantity);
                    }
                    else
                    {
                        quantityToSend = cartItem.Quantity;
                    }


                    return new OrderItemData
                    {
                        ItemId = cartItem.Item.ItemId,
                        ItemType = cartItem.Item.ItemType,
                        Quantity = quantityToSend,
                        UnitPrice = cartItem.Item.ItemPrice,
                        ItemName = cartItem.Item.ItemName
                    };
                }).ToList();
                var result = await _orderService.PlaceOrderAsync(
                    LoggedInUser.Id,
                    DiscountAmount,
                    TransportCost,
                    orderItemsData
                );
                if (result.IsSuccess)
                {
                    SuccessMessage = result.Message + $" Cod comanda: {result.OrderId}";
                    ShoppingCart.Clear();
                    CalculateCartSubtotal();
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"A aparut o eroare la plasarea comenzii: {ex.Message}";
                Debug.WriteLine($"Place Order Error: {ex.Message}");
            }
        }
        private bool CanExecutePlaceOrder(object parameter)
        {
            return !IsGuest && ShoppingCart.Any();
        }
        private void CalculateCartSubtotal()
        {
            CartSubtotal = ShoppingCart.Sum(item => item.Item.ItemPrice * item.Quantity);
        }
        private async Task CalculateDiscountAndTransport()
        {
            if (_configuration == null) return;

            var discountThresholdAmount = _configuration.GetValue<decimal>("OrderSettings:DiscountThresholdAmount");
            var loyaltyOrderCountThreshold = _configuration.GetValue<int>("OrderSettings:LoyaltyOrderCountThreshold");
            var loyaltyTimeIntervalDays = _configuration.GetValue<int>("OrderSettings:LoyaltyTimeIntervalDays");
            var discountPercentage = _configuration.GetValue<decimal>("OrderSettings:DiscountPercentage");
            var freeTransportThreshold = _configuration.GetValue<decimal>("OrderSettings:FreeTransportThreshold");
            var transportCostValue = _configuration.GetValue<decimal>("OrderSettings:TransportCost");
            decimal calculatedDiscountAmount = 0;
            bool applyDiscount = false;
            if (CartSubtotal > discountThresholdAmount)
            {
                applyDiscount = true;
            }
            if (!IsGuest && LoggedInUser != null && _orderService != null)
            {
                try
                {
                    var recentOrderCount = await _orderService.GetOrderCountInTimeFrameAsync(LoggedInUser.Id, loyaltyTimeIntervalDays);
                    if (recentOrderCount >= loyaltyOrderCountThreshold)
                    {
                        applyDiscount = true;
                        Debug.WriteLine($"Loyalty discount applied for user {LoggedInUser.Id}. Orders in last {loyaltyTimeIntervalDays} days: {recentOrderCount}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error checking loyalty discount: {ex.Message}");
                }
            }
            if (applyDiscount)
            {
                calculatedDiscountAmount = CartSubtotal * (discountPercentage / 100m);
            }
            if (calculatedDiscountAmount > CartSubtotal)
            {
                calculatedDiscountAmount = CartSubtotal;
            }

            DiscountAmount = calculatedDiscountAmount;
            decimal calculatedTransportCost = 0;
            if ((CartSubtotal - DiscountAmount) < freeTransportThreshold)
            {
                calculatedTransportCost = transportCostValue;
            }

            TransportCost = calculatedTransportCost;
            CalculateFinalTotals();
        }
        private void CalculateFinalTotals()
        {
            CartTotal = CartSubtotal - DiscountAmount + TransportCost;
            if (CartTotal < 0) CartTotal = 0;
        }
        private async Task LoadInitialData()
        {
            await LoadMenuData();
            await LoadAllergens();
        }

        private async Task LoadMenuData()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                if (_mainViewModel == null || _mainViewModel._dbContextFactory == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real menu data.");
                    return;
                }


                using (var context = _mainViewModel._dbContextFactory.CreateDbContext())
                {
                    var menuItemsData = await context.Set<DisplayMenuItem>()
                                                   .FromSqlRaw("EXEC GetFullMenuDetails")
                                                   .ToListAsync();
                    var categoriesWithItems = new List<CategoryDisplayWrapper>();
                    var groupedItems = menuItemsData.GroupBy(item => new { item.CategoryId, item.CategoryName });

                    foreach (var group in groupedItems)
                    {
                        var categoryWrapper = categoriesWithItems.FirstOrDefault(cw => cw.Category.Id == group.Key.CategoryId);
                        if (categoryWrapper == null)
                        {
                            categoryWrapper = new CategoryDisplayWrapper(new Category { Id = group.Key.CategoryId, Name = group.Key.CategoryName });
                            categoriesWithItems.Add(categoryWrapper);
                        }
                        var orderedItems = group.OrderBy(item => item.ItemName).ToList();
                        foreach (var item in orderedItems)
                        {
                            categoryWrapper.DisplayItems.Add(item);
                        }
                    }
                    _fullMenuCategories = categoriesWithItems.OrderBy(c => c.Category.Name).ToList();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MenuCategories.Clear();
                        foreach (var category in _fullMenuCategories)
                        {
                            MenuCategories.Add(category);
                        }
                        OnPropertyChanged(nameof(MenuCategories));
                    });


                    SuccessMessage = "Meniul a fost incarcat cu succes.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea meniului: {ex.Message}";
                Debug.WriteLine($"Eroare la incarcarea meniului (SP): {ex.Message}");
            }
        }
        private async Task LoadAllergens()
        {
            try
            {
                if (_allergenService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real allergen data.");
                    return;
                }

                var allergensList = await _allergenService.GetAllAllergensAsync();

                App.Current.Dispatcher.Invoke(() =>
                {
                    Allergens.Clear();
                    foreach (var allergen in allergensList.OrderBy(a => a.Name))
                    {
                        Allergens.Add(allergen);
                    }
                    OnPropertyChanged(nameof(Allergens));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading allergens: {ex.Message}");
            }
        }
        private void FilterMenu()
        {
            if (_fullMenuCategories == null || !_fullMenuCategories.Any())
            {
                Debug.WriteLine("Full menu data not loaded yet.");
                return;
            }
            var filteredCategories = new List<CategoryDisplayWrapper>();
            foreach (var categoryWrapper in _fullMenuCategories)
            {
                var newCategoryWrapper = new CategoryDisplayWrapper(categoryWrapper.Category);
                var filteredItems = categoryWrapper.DisplayItems.Where(item =>
                {
                    bool matchesSearch = true;

                    if (SelectedSearchType == SearchType.Nume)
                    {
                        if (!string.IsNullOrWhiteSpace(SearchText))
                        {
                            bool nameContainsText = item.ItemName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;

                            if (IncludeName)
                            {
                                matchesSearch = nameContainsText;
                            }
                            else
                            {
                                matchesSearch = !nameContainsText;
                            }
                        }
                    }
                    else if (SelectedSearchType == SearchType.Alergen)
                    {
                        if (SelectedAllergen != null)
                        {
                            bool containsSelectedAllergen = item.AllergensString != null &&
                                                            item.AllergensString.IndexOf(SelectedAllergen.Name, StringComparison.OrdinalIgnoreCase) >= 0;

                            if (IncludeAllergen)
                            {
                                matchesSearch = containsSelectedAllergen;
                            }
                            else
                            {
                                matchesSearch = !containsSelectedAllergen;
                            }
                        }
                    }

                    return matchesSearch;
                }).ToList();
                foreach (var item in filteredItems.OrderBy(item => item.ItemName))
                {
                    newCategoryWrapper.DisplayItems.Add(item);
                }
                if (newCategoryWrapper.DisplayItems.Any())
                {
                    filteredCategories.Add(newCategoryWrapper);
                }
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                MenuCategories.Clear();
                foreach (var category in filteredCategories.OrderBy(c => c.Category.Name))
                {
                    MenuCategories.Add(category);
                }
                OnPropertyChanged(nameof(MenuCategories));
            });
        }
        private decimal ParseQuantityDisplay(string quantityDisplay)
        {
            if (string.IsNullOrWhiteSpace(quantityDisplay))
            {
                return 0;
            }
            var match = Regex.Match(quantityDisplay, @"(\d+(\.\d+)?)");

            if (match.Success && decimal.TryParse(match.Value, out decimal quantity))
            {
                return quantity;
            }
            Debug.WriteLine($"Warning: Could not parse numeric quantity from '{quantityDisplay}'");
            return 0;
        }


    }
    public class CartItem : ViewModelBase
    {
        public DisplayMenuItem Item { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(ItemSubtotal));
            }
        }
        public decimal ItemSubtotal => Item.ItemPrice * Quantity;

        public CartItem(DisplayMenuItem item, int quantity)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Quantity = quantity;
        }
        public CartItem()
        {
            Item = new DisplayMenuItem();
        }
    }
    public class CategoryDisplayWrapper : ViewModelBase
    {
        public Category Category { get; set; }
        public ObservableCollection<Models.Wrappers.DisplayMenuItem> DisplayItems { get; set; }
        public CategoryDisplayWrapper(Category category)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            DisplayItems = new ObservableCollection<Models.Wrappers.DisplayMenuItem>();
        }
        public CategoryDisplayWrapper()
        {
            Category = new Category();
            DisplayItems = new ObservableCollection<Models.Wrappers.DisplayMenuItem>();
        }
    }
}
