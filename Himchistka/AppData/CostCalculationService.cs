using System;
using System.Collections.Generic;
using System.Linq;

namespace Himchistka.AppData
{
    public static class CostCalculationService
    {
        public static decimal CalculateDiscountedPrice(decimal basePrice, int discountPercent)
        {
            if (basePrice < 0)
                throw new ArgumentOutOfRangeException(nameof(basePrice), "Базовая стоимость услуги не может быть отрицательной.");

            if (discountPercent < 0 || discountPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(discountPercent), "Процент скидки должен находиться в диапазоне от 0 до 100%.");

            if (discountPercent == 0)
                return Math.Round(basePrice, 2);

            decimal factor = (100m - discountPercent) / 100.0m;
            return Math.Round(basePrice * factor, 2);
        }

        public static bool ValidateQuantity(int quantity)
        {
            return quantity >= 1 && quantity <= 300;
        }

        public static bool IsHighPriceService(decimal price)
        {
            return price >= 1000.00m;
        }

        public static decimal CalculateItemSubtotal(decimal basePrice, int discountPercent, int quantity)
        {
            if (!ValidateQuantity(quantity))
                throw new ArgumentOutOfRangeException(nameof(quantity), "Количество изделий должно быть в диапазоне от 1 до 300 шт.");

            decimal finalUnitPrice = CalculateDiscountedPrice(basePrice, discountPercent);
            return Math.Round(finalUnitPrice * quantity, 2);
        }

        public static decimal CalculateOrderTotal(IEnumerable<decimal> itemSubtotals)
        {
            if (itemSubtotals == null)
                return 0m;

            return itemSubtotals.Sum();
        }
    }
}
