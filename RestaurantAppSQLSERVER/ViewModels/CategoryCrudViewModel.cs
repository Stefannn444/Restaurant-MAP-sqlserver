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
        public ObservableCollection<Category> Categories { get; set; }
        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)EditCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveCategoryCommand).RaiseCanExecuteChanged();
            }
        }
        private Category _currentCategoryForEdit;
        public Category CurrentCategoryForEdit
        {
            get => _currentCategoryForEdit;
            set
            {
                _currentCategoryForEdit = value;
                CategoryName = value?.Name ?? string.Empty;
                CategoryDescription = value?.Description ?? string.Empty;

                OnPropertyChanged(nameof(CurrentCategoryForEdit));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged();
            }
        }
        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                if (CurrentCategoryForEdit != null)
                {
                    CurrentCategoryForEdit.Name = value;
                }
                OnPropertyChanged(nameof(CategoryName));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)SaveCategoryCommand).RaiseCanExecuteChanged();
            }
        }

        private string _categoryDescription;
        public string CategoryDescription
        {
            get => _categoryDescription;
            set
            {
                _categoryDescription = value;
                if (CurrentCategoryForEdit != null)
                {
                    CurrentCategoryForEdit.Description = value;
                }
                OnPropertyChanged(nameof(CategoryDescription));
                CommandManager.InvalidateRequerySuggested();
            }
        }
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
                CommandManager.InvalidateRequerySuggested();
                ((RelayCommand)EditCategoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCategoryCommand).RaiseCanExecuteChanged();
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
        public ICommand LoadCategoriesCommand { get; }
        public ICommand AddNewCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand SaveCategoryCommand { get; }
        public ICommand CancelEditCommand { get; }

        private readonly CategoryService _categoryService;
        public CategoryCrudViewModel() : this(null)
        {
        }
        public CategoryCrudViewModel(CategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            Categories = new ObservableCollection<Category>();
            LoadCategoriesCommand = new RelayCommand(async (param) => await ExecuteLoadCategories());
            AddNewCategoryCommand = new RelayCommand(ExecuteAddNewCategory);
            EditCategoryCommand = new RelayCommand(ExecuteEditCategory, CanExecuteEditOrDeleteCategory);
            DeleteCategoryCommand = new RelayCommand(async (param) => await ExecuteDeleteCategory(), CanExecuteEditOrDeleteCategory);
            SaveCategoryCommand = new RelayCommand(async (param) => await ExecuteSaveCategory(), CanExecuteSaveCategory);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);
            CategoryName = string.Empty;
            CategoryDescription = string.Empty;
             Task.Run(async () => await ExecuteLoadCategories());
        }

        private async Task ExecuteLoadCategories()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                if (_categoryService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot load real data.");
                    return;
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
            CurrentCategoryForEdit = new Category();
            IsEditing = true;
        }

        private void ExecuteEditCategory(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            if (SelectedCategory != null)
            {
                CurrentCategoryForEdit = new Category
                {
                    Id = SelectedCategory.Id,
                    Name = SelectedCategory.Name,
                    Description = SelectedCategory.Description
                };
                IsEditing = true;
            }
        }
        private bool CanExecuteEditOrDeleteCategory(object parameter)
        {

            bool canExecute = SelectedCategory != null && !IsEditing;

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
                    if (_categoryService == null)
                    {
                        Debug.WriteLine("Running in design-time context, cannot delete real data.");
                        SelectedCategory = null;
                        return;
                    }

                    await _categoryService.DeleteCategoryAsync(SelectedCategory.Id);
                    Categories.Remove(SelectedCategory);
                    SelectedCategory = null;
                    SuccessMessage = "Categoria a fost stearsa cu succes.";
                }
                catch (InvalidOperationException ex)
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
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                ErrorMessage = "Numele categoriei este obligatoriu.";
                return;
            }

            try
            {
                if (_categoryService == null)
                {
                    Debug.WriteLine("Running in design-time context, cannot save real data.");
                    IsEditing = false;
                    SelectedCategory = null;
                    CurrentCategoryForEdit = null;
                    CategoryName = string.Empty;
                    CategoryDescription = string.Empty;
                    return;
                }
                if (CurrentCategoryForEdit != null)
                {
                    CurrentCategoryForEdit.Name = CategoryName;
                    CurrentCategoryForEdit.Description = CategoryDescription;
                }

                if (CurrentCategoryForEdit.Id == 0)
                {
                    await _categoryService.AddCategoryAsync(CurrentCategoryForEdit);
                    Categories.Add(CurrentCategoryForEdit);
                    SuccessMessage = "Categoria a fost adaugata cu succes.";
                }
                else
                {
                    await _categoryService.UpdateCategoryAsync(CurrentCategoryForEdit);
                    var existingCategory = Categories.FirstOrDefault(c => c.Id == CurrentCategoryForEdit.Id);
                    if (existingCategory != null)
                    {
                        existingCategory.Name = CurrentCategoryForEdit.Name;
                        existingCategory.Description = CurrentCategoryForEdit.Description;
                    }
                    SuccessMessage = "Categoria a fost actualizata cu succes.";
                }

                IsEditing = false;
                SelectedCategory = null;
                CurrentCategoryForEdit = null;
                CategoryName = string.Empty;
                CategoryDescription = string.Empty;
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = $"Eroare: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la salvarea categoriei: {ex.Message}";
            }
        }
        private bool CanExecuteSaveCategory(object parameter)
        {
            return IsEditing && CurrentCategoryForEdit != null && !string.IsNullOrWhiteSpace(CategoryName);
        }

        private void ExecuteCancelEdit(object parameter)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsEditing = false;
            CurrentCategoryForEdit = null;
            CategoryName = string.Empty;
            CategoryDescription = string.Empty;
            SelectedCategory = null;
        }
    }
}
