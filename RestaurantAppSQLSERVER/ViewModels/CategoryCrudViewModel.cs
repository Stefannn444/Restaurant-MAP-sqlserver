using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;

namespace RestaurantAppSQLSERVER.ViewModels
{
    public class CategoryCrudViewModel : ViewModelBase
    {
        // Colectie pentru lista de categorii afisate
        public ObservableCollection<Category> Categories { get; set; }

        // Proprietate pentru selectia curenta in DataGrid
        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                // Actualizeaza starea butoanelor Edit/Delete cand selectia se schimba
                CommandManager.InvalidateRequerySuggested(); // Notificare globala (standard)

                // Notificare EXPLICITA pentru command-urile Edit si Delete
                ((RelayCommand)EditCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCategoryCommand).RaiseCanExecuteChanged();

                // Notificare si pentru SaveCommand (desi nu se aplica direct la selectie, e bine sa fie consistenta)
                ((RelayCommand)SaveCategoryCommand).RaiseCanExecuteChanged();
            }
        }

        // Proprietate pentru obiectul Category folosit pentru editare/adaugare in formular
        private Category _currentCategoryForEdit;
        public Category CurrentCategoryForEdit
        {
            get => _currentCategoryForEdit;
            set
            {
                _currentCategoryForEdit = value;
                // Actualizeaza proprietatile individuale folosite pentru binding in formular
                CategoryName = value?.Name ?? string.Empty;
                CategoryDescription = value?.Description ?? string.Empty;

                OnPropertyChanged(nameof(CurrentCategoryForEdit));
                // Notifica CommandManager cand obiectul de editare se schimba
                CommandManager.InvalidateRequerySuggested();
                // Notificare EXPLICITA pentru command-urile afectate de starea de editare
                ((RelayCommand)SaveCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged(); // CancelCommand state also depends on IsEditing
            }
        }

        // Proprietati individuale pentru binding in formularul de editare/adaugare
        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                // Actualizeaza si obiectul CurrentCategoryForEdit
                if (CurrentCategoryForEdit != null)
                {
                    CurrentCategoryForEdit.Name = value;
                }
                OnPropertyChanged(nameof(CategoryName));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveCategoryCommand).RaiseCanExecuteChanged(); // Notifica SaveCommand
            }
        }

        private string _categoryDescription;
        public string CategoryDescription
        {
            get => _categoryDescription;
            set
            {
                _categoryDescription = value;
                // Actualizeaza si obiectul CurrentCategoryForEdit
                if (CurrentCategoryForEdit != null)
                {
                    CurrentCategoryForEdit.Description = value;
                }
                OnPropertyChanged(nameof(CategoryDescription));
                CommandManager.InvalidateRequerySuggested();
                // Notifica SaveCommand (daca descrierea afecteaza validarea, desi in cazul nostru nu)
                // ((RelayCommand)SaveCategoryCommand).RaiseCanExecuteChanged();
            }
        }


        // Starea UI-ului (ex: vizualizare lista, editare, adaugare)
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
                // Notifica CommandManager cand starea de editare se schimba
                CommandManager.InvalidateRequerySuggested();
                // Notificare EXPLICITA pentru command-urile Edit, Delete si Save
                // Acestea sunt afectate direct de starea IsEditing
                ((RelayCommand)EditCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged(); // CancelCommand state also depends on IsEditing
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

        // Command-uri pentru operatiile CRUD
        public ICommand LoadCategoriesCommand { get; }
        public ICommand AddNewCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand SaveCategoryCommand { get; }
        public ICommand CancelEditCommand { get; }

        private readonly CategoryService _categoryService;

        // Constructor PUBLIC FARA PARAMETRI - DOAR PENTRU DESIGN TIME
        public CategoryCrudViewModel() : this(null)
        {
        }

        // Constructorul principal - folosit la RULARE
        public CategoryCrudViewModel(CategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));

            // Initializeaza colectia
            Categories = new ObservableCollection<Category>();

            // Initializeaza command-urile
            LoadCategoriesCommand = new RelayCommand(async (param) => await ExecuteLoadCategories());
            AddNewCategoryCommand = new RelayCommand(ExecuteAddNewCategory);
            EditCategoryCommand = new RelayCommand(ExecuteEditCategory, CanExecuteEditOrDeleteCategory);
            DeleteCategoryCommand = new RelayCommand(async (param) => await ExecuteDeleteCategory(), CanExecuteEditOrDeleteCategory);
            SaveCategoryCommand = new RelayCommand(async (param) => await ExecuteSaveCategory(), CanExecuteSaveCategory);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);

            // Initializeaza proprietatile pentru formular
            CategoryName = string.Empty;
            CategoryDescription = string.Empty;

            // Incarca datele la initializarea ViewModel-ului (optional)
             Task.Run(async () => await ExecuteLoadCategories());
        }

        // --- Metode pentru Command-uri ---

        private async Task ExecuteLoadCategories()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                // Verifica daca serviciul este null (cazul design-time)
                if (_categoryService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real data.");
                    // Poti adauga date mock aici pentru design-time
                    // Categories.Clear();
                    // Categories.Add(new Category { Id = 1, Name = "Mock Categorie 1", Description = "Descriere mock 1" });
                    // Categories.Add(new Category { Id = 2, Name = "Mock Categorie 2", Description = "Descriere mock 2" });
                    return; // Iesi din metoda daca suntem in design-time
                }


                var categoriesList = await _categoryService.GetAllCategoriesAsync();
                Categories.Clear();
                foreach (var category in categoriesList)
                {
                    Categories.Add(category);
                }
                SuccessMessage = $"Au fost incarcate {Categories.Count} categorii.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea categoriilor: {ex.Message}";
            }
        }

        private void ExecuteAddNewCategory(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            CurrentCategoryForEdit = new Category(); // Creeaza o instanta noua pentru adaugare
            IsEditing = true; // Trece in modul editare/adaugare
                              // Proprietatile individuale sunt actualizate prin setter-ul CurrentCategoryForEdit
        }

        private void ExecuteEditCategory(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedCategory != null)
            {
                // Creeaza o copie a obiectului selectat pentru editare
                CurrentCategoryForEdit = new Category
                {
                    Id = SelectedCategory.Id,
                    Name = SelectedCategory.Name,
                    Description = SelectedCategory.Description
                };
                IsEditing = true; // Trece in modul editare
                                  // Proprietatile individuale sunt actualizate prin setter-ul CurrentCategoryForEdit
            }
        }

        // Determina daca butoanele Edit/Delete pot fi executate
        private bool CanExecuteEditOrDeleteCategory(object parameter)
        {
            // Butoanele Edit/Delete sunt active doar daca:
            // 1. Un alergen este selectat in DataGrid (SelectedCategory != null)
            // 2. NU esti deja in modul de adaugare/editare (IsEditing este false)

            bool canExecute = SelectedCategory != null && !IsEditing;

            // Adauga aceasta linie pentru a vedea valorile in Output Window (optional)
            // Debug.WriteLine($"CanExecuteEditOrDeleteCategory: SelectedCategory is null? {SelectedCategory == null}, IsEditing={IsEditing}, Result={canExecute}");

            return canExecute;
        }

        private async Task ExecuteDeleteCategory()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedCategory != null)
            {
                try
                {
                    // Verifica daca serviciul este null (cazul design-time)
                    if (_categoryService == null)
                    {
                        Debug.WriteLine("Running in design-time context, cannot delete real data.");
                        // Simuleaza stergerea din colectia mock
                        // Categories.Remove(SelectedCategory);
                        SelectedCategory = null;
                        return;
                    }


                    // Confirma stergerea (optional)
                    // bool confirm = await _mainViewModel.ConfirmActionAsync($"Sunteti sigur ca doriti sa stergeti categoria '{SelectedCategory.Name}'?");
                    // if (!confirm) return;

                    await _categoryService.DeleteCategoryAsync(SelectedCategory.Id);
                    Categories.Remove(SelectedCategory); // Sterge din colectia locala
                    SelectedCategory = null; // Deselecteaza
                    SuccessMessage = "Categoria a fost stearsa cu succes.";
                }
                catch (InvalidOperationException ex) // Prinde exceptia specifica din serviciu
                {
                    ErrorMessage = $"Eroare: {ex.Message}";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Eroare la stergerea categoriei: {ex.Message}";
                }
            }
        }

        private async Task ExecuteSaveCategory()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            // Validare simpla - acum verificam CategoryName
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                ErrorMessage = "Numele categoriei este obligatoriu.";
                return;
            }

            try
            {
                // Verifica daca serviciul este null (cazul design-time)
                if (_categoryService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot save real data.");
                    // Simuleaza salvarea in colectia mock
                    // if (CurrentCategoryForEdit.Id == 0) { CurrentCategoryForEdit.Id = Categories.Count + 1; Categories.Add(CurrentCategoryForEdit); }
                    // else { var existing = Categories.FirstOrDefault(c => c.Id == CurrentCategoryForEdit.Id); if (existing != null) { existing.Name = CurrentCategoryForEdit.Name; existing.Description = CurrentCategoryForEdit.Description; } }
                    IsEditing = false;
                    SelectedCategory = null;
                    CurrentCategoryForEdit = null;
                    CategoryName = string.Empty;
                    CategoryDescription = string.Empty;
                    return;
                }


                // Make sure CurrentCategoryForEdit properties are updated from individual properties
                if (CurrentCategoryForEdit != null)
                {
                    CurrentCategoryForEdit.Name = CategoryName;
                    CurrentCategoryForEdit.Description = CategoryDescription;
                }

                if (CurrentCategoryForEdit.Id == 0) // Adaugare
                {
                    await _categoryService.AddCategoryAsync(CurrentCategoryForEdit);
                    // Update the Id of the added object from the database
                    Categories.Add(CurrentCategoryForEdit); // Adauga in colectia locala
                    SuccessMessage = "Categoria a fost adaugata cu succes.";
                }
                else // Editare
                {
                    await _categoryService.UpdateCategoryAsync(CurrentCategoryForEdit);
                    // Actualizeaza obiectul in colectia locala
                    var existingCategory = Categories.FirstOrDefault(c => c.Id == CurrentCategoryForEdit.Id);
                    if (existingCategory != null)
                    {
                        existingCategory.Name = CurrentCategoryForEdit.Name;
                        existingCategory.Description = CurrentCategoryForEdit.Description;
                    }
                    SuccessMessage = "Categoria a fost actualizata cu succes.";
                }

                IsEditing = false; // Iese din modul editare/adaugare
                SelectedCategory = null; // Deselecteaza
                CurrentCategoryForEdit = null; // Curata formularul
                CategoryName = string.Empty; // Curata campurile individuale
                CategoryDescription = string.Empty;

                // Optional: Reincarca lista completa dupa salvare
                // await ExecuteLoadCategories();
            }
            catch (InvalidOperationException ex) // Prinde exceptia specifica din serviciu
            {
                ErrorMessage = $"Eroare: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la salvarea categoriei: {ex.Message}";
            }
        }

        // Determina daca butonul Save poate fi executat
        private bool CanExecuteSaveCategory(object parameter)
        {
            // Command-ul este activ doar daca suntem in modul editare/adaugare si CurrentCategoryForEdit nu este null
            // si numele nu este gol.
            return IsEditing && CurrentCategoryForEdit != null && !string.IsNullOrWhiteSpace(CategoryName);
        }

        private void ExecuteCancelEdit(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsEditing = false;
            CurrentCategoryForEdit = null;
            CategoryName = string.Empty; // Curata campurile individuale
            CategoryDescription = string.Empty;
            SelectedCategory = null;
            // The setters for IsEditing and CurrentCategoryForEdit will trigger CommandManager.InvalidateRequerySuggested() and explicit RaiseCanExecuteChanged
        }
    }
}
