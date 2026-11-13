using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Himchistka.AppData
{
    public class AppConnect
    {
        public static himchistkaEntities model0db;
        public static user user;

        public static bool SelectProductMode;
        public static order EditingOrder;
    }

    // ============================================================
    //  РАСШИРЕНИЯ СУЩНОСТЕЙ EF (partial-классы)
    // ============================================================

    public partial class user
    {
        public string FIO => $"{surname} {name} {patronymic}".Trim();
        public string RoleName => role?.role1 ?? "Гость";
    }

    public partial class role
    {
        public string Name => role1;
    }

    public partial class category
    {
        public string Name => category1;
    }

    public partial class material
    {
        public string Name => material1;
    }

    public partial class status
    {
        public string Name => status1;
    }

    public partial class unit
    {
        public string Name => unit1;
    }

    public partial class product
    {
        public bool HasDiscount => discount.HasValue && discount.Value > 0;

        public decimal FinalPrice => HasDiscount
            ? CostCalculationService.CalculateDiscountedPrice(price, discount.Value)
            : (decimal)price;

        public bool IsHighPrice => CostCalculationService.IsHighPriceService(FinalPrice);

        public Brush PriceBrush => IsHighPrice
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0D47A1"));

        public string DiscountText => HasDiscount ? $"-{discount}%" : "";

        public Brush RowColor => HasDiscount
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"))
            : new SolidColorBrush(Colors.White);

        public string ImagePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(image))
                    return "/Images/picture.png";
                return $"/Images/{image}";
            }
        }

        public string CategoryName => category?.category1 ?? "Без категории";

        public string MaterialName => material?.material1 ?? "Любой";
    }

    public class OrderItemInfo
    {
        public string ProductName { get; set; }
        public string CategoryAndMaterial { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;
        public string StatusName { get; set; }
        public string StatusColor { get; set; }
    }

    public partial class order
    {
        public string ClientFIO => user != null ? user.FIO : "Не указан";

        public decimal TotalSum => productOrder != null && productOrder.Any()
            ? productOrder.Sum(po => po.product != null ? po.product.FinalPrice : 0m)
            : 0m;

        public static string GetStatusColor(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName)) return "#1976D2";
            if (statusName.IndexOf("Отменен", StringComparison.OrdinalIgnoreCase) >= 0) return "#E53935";
            if (statusName.IndexOf("Готов", StringComparison.OrdinalIgnoreCase) >= 0) return "#43A047";
            if (statusName.IndexOf("Выдан", StringComparison.OrdinalIgnoreCase) >= 0) return "#546E7A";
            if (statusName.IndexOf("обработк", StringComparison.OrdinalIgnoreCase) >= 0) return "#FB8C00";
            return "#1976D2";
        }

        public string OrderStatus
        {
            get
            {
                if (productOrder == null || !productOrder.Any()) return "Новый";
                var distinctStatuses = productOrder
                    .Select(p => p.status?.status1)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .ToList();
                if (distinctStatuses.Count == 0) return "Новый";
                if (distinctStatuses.Count == 1) return distinctStatuses[0];
                return string.Join(", ", distinctStatuses);
            }
        }

        public string OrderStatusColor => GetStatusColor(OrderStatus);

        public List<OrderItemInfo> GroupedItems
        {
            get
            {
                if (productOrder == null || !productOrder.Any()) return new List<OrderItemInfo>();
                return productOrder
                    .GroupBy(po => new
                    {
                        po.productId,
                        Name = po.product != null ? po.product.name : "Услуга #" + po.productId,
                        CatMat = po.product != null ? (po.product.CategoryName + " | " + po.product.MaterialName) : "",
                        Price = po.product != null ? po.product.FinalPrice : 0m,
                        Status = po.status != null ? po.status.status1 : "Новый"
                    })
                    .Select(g => new OrderItemInfo
                    {
                        ProductName = g.Key.Name,
                        CategoryAndMaterial = g.Key.CatMat,
                        Quantity = g.Count(),
                        Price = g.Key.Price,
                        StatusName = g.Key.Status,
                        StatusColor = GetStatusColor(g.Key.Status)
                    })
                    .ToList();
            }
        }
    }

    public partial class productOrder
    {
        public decimal Sum => product != null ? product.FinalPrice : 0m;
        public string StatusName => status?.status1 ?? "Неизвестно";
        public string UnitName => unit?.unit1 ?? "шт.";
    }

    // ============================================================
    //  КОРЗИНА ЗАКАЗА
    // ============================================================

    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string MaterialName { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public int? Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal Sum => Quantity * FinalPrice;
    }

    public static class Cart
    {
        public static List<CartItem> Items { get; } = new List<CartItem>();

        public static decimal TotalSum()
        {
            decimal s = 0;
            foreach (var i in Items) s += i.Sum;
            return s;
        }

        public static void Clear() => Items.Clear();
    }
}
