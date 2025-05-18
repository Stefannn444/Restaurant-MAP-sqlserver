using RestaurantAppSQLSERVER.Services;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;

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
                ((RelayCommand)EditCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCategoryCommand).RaiseCanExecuteChanged();
                // Daca treci in modul editare la selectie, actualizeaza si campurile de editare
                // CopySelectedCategoryToEditFields();
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
                OnPropertyChanged(nameof(CurrentCategoryForEdit));
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
                // Notifica command-urile care depind de starea de editare
                ((RelayCommand)SaveCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged();
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

            // Incarca datele la initializarea ViewModel-ului (optional)
            // Task.Run(async () => await ExecuteLoadCategories());
        }

        // --- Metode pentru Command-uri ---

        private async Task ExecuteLoadCategories()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
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
            }
        }

        // Determina daca butoanele Edit/Delete pot fi executate
        private bool CanExecuteEditOrDeleteCategory(object parameter)
        {
            return SelectedCategory != null && !IsEditing;
        }

        private async Task ExecuteDeleteCategory()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedCategory != null)
            {
                try
                {
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

            // Validare simpla
            if (CurrentCategoryForEdit == null || string.IsNullOrWhiteSpace(CurrentCategoryForEdit.Name))
            {
                ErrorMessage = "Numele categoriei este obligatoriu.";
                return;
            }

            try
            {
                if (CurrentCategoryForEdit.Id == 0) // Adaugare
                {
                    await _categoryService.AddCategoryAsync(CurrentCategoryForEdit);
                    Categories.Add(CurrentCategoryForEdit); // Adauga in colectia locala
                    SuccessMessage = "Categoria a fost adaugata cu succes.";
                }
                else // Editare
                {
                    await _categoryService.UpdateCategoryAsync(CurrentCategoryForEdit);
                    // Actualizeaza obiectul in colectia locala (gaseste si inlocuieste sau copiaza proprietatile)
                    var existingCategory = Categories.FirstOrDefault(c => c.Id == CurrentCategoryForEdit.Id);
                    if (existingCategory != null)
                    {
                        // Copiem proprietatile pentru a actualiza obiectul existent in colectie
                        existingCategory.Name = CurrentCategoryForEdit.Name;
                        existingCategory.Description = CurrentCategoryForEdit.Description;
                        // Notificare individuala daca este necesar: OnPropertyChanged(nameof(Categories));
                    }
                    SuccessMessage = "Categoria a fost actualizata cu succes.";
                }

                IsEditing = false; // Iese din modul editare/adaugare
                SelectedCategory = null; // Deselecteaza
                CurrentCategoryForEdit = null; // Curata formularul

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
            return IsEditing && CurrentCategoryForEdit != null && !string.IsNullOrWhiteSpace(CurrentCategoryForEdit.Name);
        }

        private void ExecuteCancelEdit(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsEditing = false;
            CurrentCategoryForEdit = null;
            SelectedCategory = null;
        }
    }
}
