using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konditerka.AppData
{
    public class BasketItemViewModel
    {
        public int IdBasketCatalog { get; set; }
        public int IdCatalog { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal PositionTotal => Price * Quantity;
    }

    public static class BasketManager
    {
        public static bool IsUserAuthorized()
        {
            return CurrentUser.User != null;
        }

        public static Baskets GetOrCreateCurrentBasket()
        {
            if (!IsUserAuthorized())
            {
                throw new InvalidOperationException("Пользователь не авторизован.");
            }

            int userId = CurrentUser.User.IdUser;
            Baskets basket = AppConnect.model0db.Baskets
                .Include(x => x.BasketsCatalogs)
                .FirstOrDefault(x => x.IdUser == userId && !x.IsOrdered);

            if (basket != null)
            {
                return basket;
            }

            basket = new Baskets
            {
                IdUser = userId,
                CreateDate = DateTime.Now,
                IsOrdered = false,
                TotalPrice = 0m
            };

            AppConnect.model0db.Baskets.Add(basket);
            AppConnect.model0db.SaveChanges();
            return basket;
        }

        public static void AddToBasket(Catalogs product, int quantity = 1)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            if (quantity <= 0)
            {
                quantity = 1;
            }

            Baskets basket = GetOrCreateCurrentBasket();

            BasketsCatalogs basketItem = AppConnect.model0db.BasketsCatalogs
                .FirstOrDefault(x => x.IdBasket == basket.IdBasket && x.IdCatalog == product.IdCatalog);

            if (basketItem == null)
            {
                basketItem = new BasketsCatalogs
                {
                    IdBasket = basket.IdBasket,
                    IdCatalog = product.IdCatalog,
                    Quantity = quantity
                };
                AppConnect.model0db.BasketsCatalogs.Add(basketItem);
            }
            else
            {
                basketItem.Quantity += quantity;
            }

            RecalculateBasketTotal(basket.IdBasket);
            AppConnect.model0db.SaveChanges();
        }

        public static List<BasketItemViewModel> GetCurrentBasketItems()
        {
            Baskets basket = GetOrCreateCurrentBasket();

            List<BasketItemViewModel> items = AppConnect.model0db.BasketsCatalogs
                .Where(x => x.IdBasket == basket.IdBasket)
                .Select(x => new BasketItemViewModel
                {
                    IdBasketCatalog = x.IdBasketCatalog,
                    IdCatalog = x.IdCatalog,
                    ProductName = x.Catalogs.Product,
                    Price = x.Catalogs.Price,
                    Quantity = x.Quantity
                })
                .ToList();

            return items;
        }

        public static decimal GetCurrentBasketTotal()
        {
            Baskets basket = GetOrCreateCurrentBasket();
            RecalculateBasketTotal(basket.IdBasket);
            return basket.TotalPrice;
        }

        public static void RemoveItem(int basketCatalogId)
        {
            BasketsCatalogs item = AppConnect.model0db.BasketsCatalogs.FirstOrDefault(x => x.IdBasketCatalog == basketCatalogId);
            if (item == null)
            {
                return;
            }

            int basketId = item.IdBasket;
            AppConnect.model0db.BasketsCatalogs.Remove(item);
            RecalculateBasketTotal(basketId);
            AppConnect.model0db.SaveChanges();
        }

        public static void ClearCurrentBasket()
        {
            Baskets basket = GetOrCreateCurrentBasket();
            List<BasketsCatalogs> items = AppConnect.model0db.BasketsCatalogs.Where(x => x.IdBasket == basket.IdBasket).ToList();
            if (items.Count == 0)
            {
                return;
            }

            AppConnect.model0db.BasketsCatalogs.RemoveRange(items);
            basket.TotalPrice = 0m;
            AppConnect.model0db.SaveChanges();
        }

        public static void RecalculateBasketTotal(int basketId)
        {
            Baskets basket = AppConnect.model0db.Baskets.FirstOrDefault(x => x.IdBasket == basketId);
            if (basket == null)
            {
                return;
            }

            decimal total = AppConnect.model0db.BasketsCatalogs
                .Where(x => x.IdBasket == basketId)
                .Select(x => x.Catalogs.Price * x.Quantity)
                .DefaultIfEmpty(0m)
                .Sum();

            basket.TotalPrice = total;
        }
    }
}
