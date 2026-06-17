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
    public partial class CheckoutPage : Page
    {
        private readonly List<BasketItemViewModel> _items;
        private readonly decimal _basketTotal;

        public CheckoutPage(List<BasketItemViewModel> items, decimal basketTotal)
        {
            InitializeComponent();
            _items = items;
            _basketTotal = basketTotal;
            DeliveryList.ItemsSource = AppConnect.model0db.DeliveryMethods.ToList();
            PaymentList.ItemsSource = AppConnect.model0db.PaymentMethods.ToList();
            DeliveryList.SelectedIndex = 0;
            PaymentList.SelectedIndex = 0;
            WindowSizeHelper.SetMinSize(500, 800);
            UpdateTotal();
        }

        private void DeliveryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeliveryList.SelectedItem is DeliveryMethods method)
            {
                bool needsAddress = method.PriceDelivery > 0;
                AddressPanel.Visibility = needsAddress ? Visibility.Visible : Visibility.Collapsed;
            }
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal delivery = 0;
            if (DeliveryList.SelectedItem is DeliveryMethods m)
                delivery = m.PriceDelivery;

            TotalBlock.Text = $"Итого: {_basketTotal + delivery:N2} ₽" +
                              (delivery > 0 ? $"  (+ {delivery:N0} ₽ доставка)" : "");
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeliveryList.SelectedItem == null)
            {
                MessageBox.Show("Выберите способ доставки.");
                return;
            }
            if (PaymentList.SelectedItem == null)
            {
                MessageBox.Show("Выберите способ оплаты.");
                return;
            }

            var delivery = (DeliveryMethods)DeliveryList.SelectedItem;
            var payment = (PaymentMethods)PaymentList.SelectedItem;

            if (delivery.PriceDelivery > 0 && string.IsNullOrWhiteSpace(AddressBox.Text))
            {
                MessageBox.Show("Введите адрес доставки.");
                return;
            }

            try
            {
                decimal total = _basketTotal + delivery.PriceDelivery;
                Orders order = null;

                using (var transaction = AppConnect.model0db.Database.BeginTransaction())
                {
                    var defaultStatus = AppConnect.model0db.StatusOrders
                        .OrderBy(s => s.IdStatusOrder)
                        .FirstOrDefault()
                        ?? throw new InvalidOperationException("Не найден статус заказа.");

                    order = new Orders
                    {
                        IdUser = CurrentUser.User.IdUser,
                        IdStatusOrder = defaultStatus.IdStatusOrder,
                        IdPaymentMethod = payment.IdPaymentMethod,
                        IdDeliveryMethod = delivery.IdDeliveryMethod,
                        Data = DateTime.Now,
                        Price = total,
                        DeliveryAddress = AddressBox?.Text.Trim(),
                        Comment = CommentBox.Text.Trim()
                    };

                    AppConnect.model0db.Orders.Add(order);
                    AppConnect.model0db.SaveChanges();

                    foreach (var item in _items)
                    {
                        AppConnect.model0db.OrdersCatalogs.Add(new OrdersCatalogs
                        {
                            IdOrder = order.IdOrder,
                            IdCatalog = item.IdCatalog,
                            Quantity = item.Quantity,
                            PriceAtOrder = item.Price
                        });
                    }

                    var basket = BasketManager.GetOrCreateCurrentBasket();
                    basket.IsOrdered = true;
                    basket.TotalPrice = total;

                    AppConnect.model0db.SaveChanges();
                    transaction.Commit();
                }

                BasketManager.ClearCurrentBasket();

                string pdfPath = GenerateAndSaveCheck(order, delivery, payment, _items);

                MessageBox.Show(
                    $"Заказ №{order.IdOrder} успешно оформлен!\n" +
                    $"Доставка: {delivery.NameDeliveryMethod}\n" +
                    $"Оплата: {payment.NamePaymentMethod}\n" +
                    $"Сумма: {total:N2} ₽\n\n" +
                    $"Чек сохранён:\n{pdfPath}",
                    "Заказ принят", MessageBoxButton.OK, MessageBoxImage.Information);

                AppFrame.framemain?.Navigate(new PageOutput());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка оформления заказа: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string GenerateAndSaveCheck(Orders order, DeliveryMethods delivery,
            PaymentMethods payment, List<BasketItemViewModel> items)
        {
            string checksDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Checks");
            Directory.CreateDirectory(checksDir);
            string defaultName = $"Check_Order_{order.IdOrder}.pdf";
            string targetPath = Path.Combine(checksDir, defaultName);

            var dlg = new SaveFileDialog
            {
                Title = "Сохранить чек",
                Filter = "PDF-файл (*.pdf)|*.pdf",
                FileName = defaultName,
                InitialDirectory = checksDir
            };
            if (dlg.ShowDialog() == true)
                targetPath = dlg.FileName;

            CreatePdf(targetPath, order, delivery, payment, items);
            return targetPath;
        }

        private void CreatePdf(string path, Orders order, DeliveryMethods delivery,
            PaymentMethods payment, List<BasketItemViewModel> items)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var document = new Document(PageSize.A4, 40, 40, 40, 40))
            {
                PdfWriter.GetInstance(document, stream);
                document.Open();

                var titleFont = MakeFont(16, iTextSharp.text.Font.BOLD);
                var headerFont = MakeFont(12, iTextSharp.text.Font.BOLD);
                var normalFont = MakeFont(11, iTextSharp.text.Font.NORMAL);
                var smallFont = MakeFont(9, iTextSharp.text.Font.NORMAL);

                string logo = FindFile(new[] { "Images/logo.png", "Images/logo.jpg",
                                               "Images/Logo.png", "Images/Logo.jpg" });
                if (logo != null)
                {
                    var img = iTextSharp.text.Image.GetInstance(logo);
                    img.ScaleToFit(100f, 100f);
                    img.Alignment = Element.ALIGN_CENTER;
                    document.Add(img);
                }

                AddParagraph(document, "Кондитерская — Чек заказа", titleFont, Element.ALIGN_CENTER);
                document.Add(new Paragraph(" "));

                AddParagraph(document, $"Заказ №{order.IdOrder}", headerFont, Element.ALIGN_LEFT);
                AddParagraph(document, $"Дата:       {order.Data:dd.MM.yyyy HH:mm}", normalFont, Element.ALIGN_LEFT);
                AddParagraph(document, $"Клиент:     {CurrentUser.User.NameUser}", normalFont, Element.ALIGN_LEFT);
                AddParagraph(document, $"Доставка:   {delivery.NameDeliveryMethod}" +
                    (delivery.PriceDelivery > 0 ? $" (+{delivery.PriceDelivery:N0} ₽)" : " (бесплатно)"),
                    normalFont, Element.ALIGN_LEFT);

                if (!string.IsNullOrWhiteSpace(order.DeliveryAddress))
                    AddParagraph(document, $"Адрес:      {order.DeliveryAddress}", normalFont, Element.ALIGN_LEFT);

                AddParagraph(document, $"Оплата:     {payment.NamePaymentMethod}", normalFont, Element.ALIGN_LEFT);

                if (!string.IsNullOrWhiteSpace(order.Comment))
                    AddParagraph(document, $"Комментарий: {order.Comment}", smallFont, Element.ALIGN_LEFT);

                document.Add(new Paragraph(" "));

                var table = new PdfPTable(4) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 44f, 16f, 20f, 20f });

                AddCell(table, "Товар", headerFont, true);
                AddCell(table, "Кол-во", headerFont, true);
                AddCell(table, "Цена", headerFont, true);
                AddCell(table, "Сумма", headerFont, true);

                foreach (var item in items)
                {
                    AddCell(table, item.ProductName, normalFont, false);
                    AddCell(table, item.Quantity.ToString(), normalFont, false);
                    AddCell(table, $"{item.Price:N2} ₽", normalFont, false);
                    AddCell(table, $"{item.PositionTotal:N2} ₽", normalFont, false);
                }

                document.Add(table);
                document.Add(new Paragraph(" "));

                decimal basketSum = items.Sum(x => x.PositionTotal);
                if (delivery.PriceDelivery > 0)
                    AddParagraph(document, $"Товары:   {basketSum:N2} ₽", normalFont, Element.ALIGN_RIGHT);

                AddParagraph(document, $"ИТОГО:    {order.Price:N2} ₽", titleFont, Element.ALIGN_RIGHT);
                document.Add(new Paragraph(" "));

                byte[] qrBytes = GenerateQr(order, delivery, payment, items);
                var qrImg = iTextSharp.text.Image.GetInstance(qrBytes);
                qrImg.ScaleToFit(130f, 130f);
                qrImg.Alignment = Element.ALIGN_CENTER;

                AddParagraph(document, "QR-код заказа:", smallFont, Element.ALIGN_CENTER);
                document.Add(qrImg);
                AddParagraph(document,
                    "Отсканируйте для проверки информации о заказе",
                    smallFont, Element.ALIGN_CENTER);

                document.Close();
            }
        }

        private byte[] GenerateQr(Orders order, DeliveryMethods delivery,
            PaymentMethods payment, List<BasketItemViewModel> items)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ЗАКАЗ #{order.IdOrder}");
            sb.AppendLine($"Дата: {order.Data:dd.MM.yyyy HH:mm}");
            sb.AppendLine($"Клиент: {CurrentUser.User.NameUser}");
            sb.AppendLine($"Доставка: {delivery.NameDeliveryMethod}");
            sb.AppendLine($"Оплата: {payment.NamePaymentMethod}");
            if (!string.IsNullOrWhiteSpace(order.DeliveryAddress))
                sb.AppendLine($"Адрес: {order.DeliveryAddress}");
            sb.AppendLine("---");
            foreach (var item in items)
                sb.AppendLine($"{item.ProductName} x{item.Quantity} = {item.PositionTotal:N2} руб.");
            sb.AppendLine("---");
            sb.AppendLine($"ИТОГО: {order.Price:N2} руб.");

            using (var gen = new QRCodeGenerator())
            using (var data = gen.CreateQrCode(sb.ToString(), QRCodeGenerator.ECCLevel.Q))
            {
                var png = new PngByteQRCode(data);
                return png.GetGraphic(20);
            }
        }

        private static void AddParagraph(Document doc, string text,
            iTextSharp.text.Font font, int align)
        {
            var p = new Paragraph(new Phrase(text, font)) { Alignment = align };
            doc.Add(p);
        }

        private static void AddCell(PdfPTable table, string text,
            iTextSharp.text.Font font, bool isHeader)
        {
            var cell = new PdfPCell(new Phrase(text ?? "", font))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5f,
                BackgroundColor = isHeader
                    ? new BaseColor(64, 0, 70)
                    : BaseColor.WHITE
            };
            if (isHeader)
                cell.Phrase.Font.Color = BaseColor.WHITE;
            table.AddCell(cell);
        }

        private static iTextSharp.text.Font MakeFont(float size, int style)
        {
            string fontPath = FindFontPath();
            if (fontPath != null)
            {
                var bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                return new iTextSharp.text.Font(bf, size, style);
            }
            return style == iTextSharp.text.Font.BOLD
                ? FontFactory.GetFont(FontFactory.HELVETICA_BOLD, size)
                : FontFactory.GetFont(FontFactory.HELVETICA, size);
        }

        private static string FindFontPath()
        {
            string fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            foreach (var name in new[] { "arial.ttf", "times.ttf", "calibri.ttf", "verdana.ttf" })
            {
                string p = Path.Combine(fontsDir, name);
                if (File.Exists(p)) return p;
            }
            return null;
        }

        private static string FindFile(string[] candidates)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var rel in candidates)
            {
                string full = Path.Combine(baseDir, rel);
                if (File.Exists(full)) return full;
            }
            return null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain?.Navigate(new BasketPage());
        }
    }
}