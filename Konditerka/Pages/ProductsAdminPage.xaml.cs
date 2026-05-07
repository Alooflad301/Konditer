using Konditerka.AppData;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Konditerka.Pages
{
    public partial class ProductsAdminPage : Page
    {
        private Catalogs _selectedProduct;
        public Catalogs catalogs = new Catalogs();

        public ProductsAdminPage(Catalogs catalog)
        {
            InitializeComponent();
            CategoryBox.ItemsSource = AppConnect.model0db.Categories.ToList();
            if (catalog != null)
            {
                catalogs = catalog;
            }
            DataContext = catalogs;
            RefreshData();
        }

        private void RefreshData()
        {
            ProductsGrid.ItemsSource = AppConnect.model0db.Catalogs.ToList();
            ProductsGrid.SelectedItem = null;
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedProduct = null;
            ProductNameBox.Text = string.Empty;
            CategoryBox.SelectedIndex = -1;
            PriceBox.Text = string.Empty;
            DescriptionBox.Text = string.Empty;
            PhotoPathBox.Text = string.Empty;
        }

        private bool ValidateForm()
        {
            if (AppConnect.model0db.Catalogs.Count(x => x.Product == ProductNameBox.Text) > 0)
            {
                MessageBox.Show("Изделие с таким название уже существует!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (string.IsNullOrWhiteSpace(ProductNameBox.Text))
            {
                MessageBox.Show("Введите название товара", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (CategoryBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DescriptionBox.Text))
            {
                MessageBox.Show("Введите описание товара", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (DescriptionBox.Text.Length > 1000)
            {
                MessageBox.Show("Описание товара не может быть больше 1000 символов", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (!decimal.TryParse(PriceBox.Text, out var price) || price <= 0)
            {
                MessageBox.Show("Цена должна быть больше 0", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            return true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateForm())
                {
                    return;
                }

                var catalogsobj = new Catalogs()
                {
                    Product = ProductNameBox.Text.Trim(),
                    IdCategory = (int)CategoryBox.SelectedValue,
                    Price = decimal.Parse(PriceBox.Text),
                    Descripton = DescriptionBox.Text.Trim(),
                    PhotoPath = PhotoPathBox.Text.Trim(),
                };

                AppConnect.model0db.Catalogs.Add(catalogsobj);
                AppConnect.model0db.SaveChanges();
                MessageBox.Show("Данные успешно добавлены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении данных!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProduct == null)
            {
                MessageBox.Show("Выберите товар для редактирования");
                return;
            }

            if (!ValidateForm())
            {
                return;
            }

            _selectedProduct.Product = ProductNameBox.Text.Trim();
            _selectedProduct.IdCategory = CategoryBox.SelectedIndex;
            _selectedProduct.Price = decimal.Parse(PriceBox.Text);
            _selectedProduct.Descripton = DescriptionBox.Text.Trim();
            _selectedProduct.PhotoPath = PhotoPathBox.Text.Trim();

            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProduct == null)
            {
                MessageBox.Show("Выберите товар для удаления");
                return;
            }

            var result = MessageBox.Show("Удалить выбранный товар?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            AppConnect.model0db.Catalogs.Remove(_selectedProduct);
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProduct = ProductsGrid.SelectedItem as Catalogs;
            if (_selectedProduct == null)
            {
                return;
            }

            ProductNameBox.Text = _selectedProduct.Product;
            CategoryBox.SelectedValue = _selectedProduct.IdCategory;
            PriceBox.Text = _selectedProduct.Price.ToString();
            DescriptionBox.Text = _selectedProduct.Descripton;
            PhotoPathBox.Text = _selectedProduct.PhotoPath;
        }

        private void PhotoPathBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*";
            dialog.Title = "Выберите изображение";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string photoName = System.IO.Path.GetFileName(dialog.FileName);
                    string imagesDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Images\\");
                    if (!Directory.Exists(imagesDirectory))
                    {
                        Directory.CreateDirectory(imagesDirectory);
                    }
                    catalogs.PhotoPath = photoName;
                    PhotoPathBox.Text = photoName;
                    MessageBox.Show("Изображение загружено: " + photoName, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Изображение не выбранно!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
