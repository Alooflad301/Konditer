using Konditerka.AppData;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Konditerka.Pages
{
    /// <summary>
    /// Универсальная страница для справочников.
    /// Заменяет CategoriesAdminPage, CitiesAdminPage и любые будущие справочники.
    ///
    /// Использование:
    ///   new DictionaryAdminPage("Единицы измерения", DictionaryAdminPage.DictionaryType.Units)
    /// </summary>
    public partial class DictionaryAdminPage : Page
    {
        public enum DictionaryType { Categories, Cities, Units, PaymentMethods, DeliveryMethods }

        private class DictItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private readonly DictionaryType _type;
        private DictItem _selected;

        public DictionaryAdminPage(string title, DictionaryType type)
        {
            InitializeComponent();
            _type = type;
            if (Parent is Frame f) {}
            Title = title;
            RefreshData();
            WindowSizeHelper.SetMinSize(400, 760);
        }


        private void RefreshData()
        {
            ItemsGrid.ItemsSource = LoadItems();
            ItemsGrid.SelectedItem = null;
            _selected = null;
            NameBox.Text = string.Empty;
        }

        private List<DictItem> LoadItems()
        {
            var list = new List<DictItem>();
            switch (_type)
            {
                case DictionaryType.Categories:
                    foreach (var x in AppConnect.model0db.Categories)
                        list.Add(new DictItem { Id = x.IdCategory, Name = x.NameCategory });
                    break;
                case DictionaryType.Cities:
                    foreach (var x in AppConnect.model0db.Cities)
                        list.Add(new DictItem { Id = x.IdCity, Name = x.NameCity });
                    break;
                case DictionaryType.Units:
                    foreach (var x in AppConnect.model0db.Units)
                        list.Add(new DictItem { Id = x.IdUnit, Name = x.NameUnit });
                    break;
                case DictionaryType.PaymentMethods:
                    foreach (var x in AppConnect.model0db.PaymentMethods)
                        list.Add(new DictItem { Id = x.IdPaymentMethod, Name = x.NamePaymentMethod });
                    break;
                case DictionaryType.DeliveryMethods:
                    foreach (var x in AppConnect.model0db.DeliveryMethods)
                        list.Add(new DictItem { Id = x.IdDeliveryMethod, Name = x.NameDeliveryMethod });
                    break;
            }
            return list;
        }


        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;
            try
            {
                string name = NameBox.Text.Trim();
                switch (_type)
                {
                    case DictionaryType.Categories:
                        AppConnect.model0db.Categories.Add(new Categories { NameCategory = name }); break;
                    case DictionaryType.Cities:
                        AppConnect.model0db.Cities.Add(new Cities { NameCity = name }); break;
                    case DictionaryType.Units:
                        AppConnect.model0db.Units.Add(new Units { NameUnit = name }); break;
                    case DictionaryType.PaymentMethods:
                        AppConnect.model0db.PaymentMethods.Add(new PaymentMethods { NamePaymentMethod = name }); break;
                    case DictionaryType.DeliveryMethods:
                        AppConnect.model0db.DeliveryMethods.Add(new DeliveryMethods { NameDeliveryMethod = name }); break;
                }
                AppConnect.model0db.SaveChanges();
                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) { MessageBox.Show("Выберите запись для редактирования."); return; }
            if (!Validate()) return;
            try
            {
                string name = NameBox.Text.Trim();
                int id = _selected.Id;
                switch (_type)
                {
                    case DictionaryType.Categories:
                        AppConnect.model0db.Categories.Find(id).NameCategory = name; break;
                    case DictionaryType.Cities:
                        AppConnect.model0db.Cities.Find(id).NameCity = name; break;
                    case DictionaryType.Units:
                        AppConnect.model0db.Units.Find(id).NameUnit = name; break;
                    case DictionaryType.PaymentMethods:
                        AppConnect.model0db.PaymentMethods.Find(id).NamePaymentMethod = name; break;
                    case DictionaryType.DeliveryMethods:
                        AppConnect.model0db.DeliveryMethods.Find(id).NameDeliveryMethod = name; break;
                }
                AppConnect.model0db.SaveChanges();
                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) { MessageBox.Show("Выберите запись для удаления."); return; }
            var result = MessageBox.Show($"Удалить \"{_selected.Name}\"?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                int id = _selected.Id;
                switch (_type)
                {
                    case DictionaryType.Categories:
                        AppConnect.model0db.Categories.Remove(AppConnect.model0db.Categories.Find(id)); break;
                    case DictionaryType.Cities:
                        AppConnect.model0db.Cities.Remove(AppConnect.model0db.Cities.Find(id)); break;
                    case DictionaryType.Units:
                        AppConnect.model0db.Units.Remove(AppConnect.model0db.Units.Find(id)); break;
                    case DictionaryType.PaymentMethods:
                        AppConnect.model0db.PaymentMethods.Remove(AppConnect.model0db.PaymentMethods.Find(id)); break;
                    case DictionaryType.DeliveryMethods:
                        AppConnect.model0db.DeliveryMethods.Remove(AppConnect.model0db.DeliveryMethods.Find(id)); break;
                }
                AppConnect.model0db.SaveChanges();
                RefreshData();
            }
            catch (Exception)
            {
                MessageBox.Show("Нельзя удалить — запись используется в других таблицах.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ItemsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = ItemsGrid.SelectedItem as DictItem;
            NameBox.Text = _selected?.Name ?? string.Empty;
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите название.");
                return false;
            }
            return true;
        }
    }
}