using Konditerka.AppData;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Konditerka.Pages
{
    public partial class CategoriesAdminPage : Page
    {
        private Categories _selectedCategory;

        public CategoriesAdminPage()
        {
            InitializeComponent();
            RefreshData();
        }

        private void RefreshData()
        {
            CategoriesGrid.ItemsSource = AppConnect.model0db.Categories.ToList();
            CategoriesGrid.SelectedItem = null;
            _selectedCategory = null;
            CategoryNameBox.Text = string.Empty;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(CategoryNameBox.Text))
            {
                MessageBox.Show("Введите название категории");
                return false;
            }

            return true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;
            AppConnect.model0db.Categories.Add(new Categories { NameCategory = CategoryNameBox.Text.Trim() });
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCategory == null)
            {
                MessageBox.Show("Выберите категорию для редактирования");
                return;
            }
            if (!ValidateForm()) return;
            _selectedCategory.NameCategory = CategoryNameBox.Text.Trim();
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCategory == null)
            {
                MessageBox.Show("Выберите категорию для удаления");
                return;
            }
            var result = MessageBox.Show("Удалить выбранную категорию?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            AppConnect.model0db.Categories.Remove(_selectedCategory);
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void CategoriesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCategory = CategoriesGrid.SelectedItem as Categories;
            if (_selectedCategory != null)
                CategoryNameBox.Text = _selectedCategory.NameCategory;
        }
    }
}
