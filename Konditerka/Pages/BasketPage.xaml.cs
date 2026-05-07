using Konditerka.AppData;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Konditerka.Pages
{
    /// <summary>
    /// Логика взаимодействия для BasketPage.xaml
    /// </summary>
    public partial class BasketPage : Page
    {
        private List<BasketItemViewModel> _items = new List<BasketItemViewModel>();

        public BasketPage()
        {
            InitializeComponent();
            LoadBasket();
        }

        private void LoadBasket()
        {
            try
            {
                _items = BasketManager.GetCurrentBasketItems();
                BasketGrid.ItemsSource = _items;
                TotalTextBlock.Text = $"Итого: {_items.Sum(x => x.PositionTotal):N2} ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки корзины: " + ex.Message);
            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            BasketItemViewModel item = (sender as Button)?.DataContext as BasketItemViewModel;
            if (item == null)
            {
                return;
            }

            BasketManager.RemoveItem(item.IdBasketCatalog);
            LoadBasket();
        }

        private void ClearBasketButton_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("Корзина уже пуста.");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Очистить корзину?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            BasketManager.ClearCurrentBasket();
            LoadBasket();
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_items.Count == 0)
                {
                    MessageBox.Show("Корзина пуста. Добавьте товары перед оформлением заказа.");
                    return;
                }

                Baskets basket = BasketManager.GetOrCreateCurrentBasket();
                decimal total = _items.Sum(x => x.PositionTotal);

                using (var transaction = AppConnect.model0db.Database.BeginTransaction())
                {
                    Orders order = new Orders
                    {
                        IdUser = CurrentUser.User.IdUser,
                        Data = DateTime.Now,
                        Price = total,
                        IdStatusOrder = GetDefaultStatusId()
                    };

                    AppConnect.model0db.Orders.Add(order);
                    AppConnect.model0db.SaveChanges();

                    foreach (BasketItemViewModel item in _items)
                    {
                        AppConnect.model0db.OrdersCatalogs.Add(new OrdersCatalogs
                        {
                            IdOrder = order.IdOrder,
                            IdCatalog = item.IdCatalog,
                            Quantity = item.Quantity
                        });
                    }

                    basket.IsOrdered = true;
                    basket.TotalPrice = total;

                    AppConnect.model0db.SaveChanges();
                    transaction.Commit();

                    string pdfPath = GenerateAndSaveCheck(order, _items);
                    BasketManager.ClearCurrentBasket();

                    MessageBox.Show($"Заказ №{order.IdOrder} успешно оформлен.\nЧек сохранен: {pdfPath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadBasket();
            }
            catch (Exception ex)
            {

            }
        }

        private int GetDefaultStatusId()
        {
            StatusOrders status = AppConnect.model0db.StatusOrders.OrderBy(x => x.IdStatusOrder).FirstOrDefault();
            if (status == null)
            {
                throw new InvalidOperationException("Не найдено ни одного статуса заказа в базе данных.");
            }

            return status.IdStatusOrder;
        }

        private string GenerateAndSaveCheck(Orders order, List<BasketItemViewModel> items)
        {
            string checksDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Checks");
            Directory.CreateDirectory(checksDirectory);

            string defaultFileName = $"Check_Order_{order.IdOrder}.pdf";
            string targetPath = Path.Combine(checksDirectory, defaultFileName);

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Title = "Сохранить чек",
                Filter = "PDF-файл (*.pdf)|*.pdf",
                FileName = defaultFileName,
                InitialDirectory = checksDirectory
            };

            if (saveDialog.ShowDialog() == true)
            {
                targetPath = saveDialog.FileName;
            }

            CreatePdfCheck(targetPath, order, items);
            return targetPath;
        }

        private void CreatePdfCheck(string pdfPath, Orders order, List<BasketItemViewModel> items)
        {
            using (FileStream stream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (Document document = new Document(PageSize.A4, 36, 36, 36, 36))
            {
                PdfWriter.GetInstance(document, stream);
                document.Open();

                string logoPath = GetLogoPath();
                if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
                {
                    iTextSharp.text.Image logoImage = iTextSharp.text.Image.GetInstance(logoPath);      
                    logoImage.ScaleToFit(120f, 120f);
                    logoImage.Alignment = Element.ALIGN_CENTER;
                    document.Add(logoImage);
                }

                iTextSharp.text.Font titleFont = CreatePdfFont(16f, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font regularFont = CreatePdfFont(11f, iTextSharp.text.Font.NORMAL);

                Paragraph title = new Paragraph(new Phrase("Чек кондитерской", titleFont));
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph(new Phrase($"Номер заказа: {order.IdOrder}", regularFont)));
                document.Add(new Paragraph(new Phrase($"Дата: {order.Data:dd.MM.yyyy HH:mm}", regularFont)));   
                document.Add(new Paragraph(" "));

                PdfPTable table = new PdfPTable(4)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 44f, 16f, 20f, 20f });

                AddCell(table, "Товар", true);
                AddCell(table, "Кол-во", true);
                AddCell(table, "Цена", true);
                AddCell(table, "Сумма", true);

                foreach (BasketItemViewModel item in items)
                {
                    AddCell(table, item.ProductName, false);
                    AddCell(table, item.Quantity.ToString(), false);
                    AddCell(table, $"{item.Price:N2} ₽", false);
                    AddCell(table, $"{item.PositionTotal:N2} ₽", false);
                }

                document.Add(table);
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(new Phrase($"ИТОГО: {order.Price:N2} ₽", titleFont)));
                document.Add(new Paragraph(" "));

                byte[] qrCodeBytes = GenerateQrCode(order, items);
                iTextSharp.text.Image qrImage = iTextSharp.text.Image.GetInstance(qrCodeBytes);
                qrImage.ScaleToFit(140f, 140f);
                qrImage.Alignment = Element.ALIGN_CENTER;
                document.Add(new Paragraph(new Phrase("QR-данные заказа:", regularFont)));
                document.Add(qrImage);

                document.Close();
            }
        }

        private static void AddCell(PdfPTable table, string text, bool isHeader)
        {
            iTextSharp.text.Font font = isHeader
                ? CreatePdfFont(10f, iTextSharp.text.Font.BOLD)
                : CreatePdfFont(10f, iTextSharp.text.Font.NORMAL);

            PdfPCell cell = new PdfPCell(new Phrase(text ?? string.Empty, font))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5f
            };

            table.AddCell(cell);
        }

        private byte[] GenerateQrCode(Orders order, List<BasketItemViewModel> items)
        {
            StringBuilder qrContent = new StringBuilder();
            qrContent.Append($"Order #{order.IdOrder} | {order.Data:dd.MM.yyyy HH:mm} | {order.Price:N2} ₽");
            qrContent.AppendLine();
            foreach (BasketItemViewModel item in items)
            {
                qrContent.AppendLine($"{item.ProductName} - {item.Quantity} шт - {item.PositionTotal:N2} ₽");
            }

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrData = qrGenerator.CreateQrCode(qrContent.ToString(), QRCodeGenerator.ECCLevel.Q))
            {
                PngByteQRCode qrCode = new PngByteQRCode(qrData);
                return qrCode.GetGraphic(20);
            }
        }

        private string GetLogoPath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidatePaths =
            {
                Path.Combine(baseDirectory, "Images", "logo.png"),
                Path.Combine(baseDirectory, "Images", "logo.jpg"),
                Path.Combine(baseDirectory, "Images", "Logo.png"),
                Path.Combine(baseDirectory, "Images", "Logo.jpg")
            };

            return candidatePaths.FirstOrDefault(File.Exists);
        }

        private static iTextSharp.text.Font CreatePdfFont(float size, int style)
        {
            BaseFont unicodeBaseFont = GetUnicodeBaseFont();
            if (unicodeBaseFont != null)
            {
                return new iTextSharp.text.Font(unicodeBaseFont, size, style);
            }

            return style == iTextSharp.text.Font.BOLD
                ? FontFactory.GetFont(FontFactory.HELVETICA_BOLD, size)
                : FontFactory.GetFont(FontFactory.HELVETICA, size);
        }

        private static BaseFont GetUnicodeBaseFont()
        {
            string regularPath = GetUnicodeFontPath();
            if (string.IsNullOrWhiteSpace(regularPath) || !File.Exists(regularPath))
            {
                return null;
            }

            return BaseFont.CreateFont(regularPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        }

        private static string GetUnicodeFontPath()
        {
            string windowsFonts = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            string[] candidates =
            {
                "arial.ttf",
                "times.ttf",
                "calibri.ttf",
                "verdana.ttf",
                "tahoma.ttf"
            };

            foreach (string fileName in candidates)
            {
                string path = Path.Combine(windowsFonts, fileName);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain.Navigate(new PageOutput());
        }
    }
}
