using System;
using System.Collections.Generic;
using System.Linq;

namespace Himchistka.AppData
{
    /// <summary>
    /// Сервис расчетной бизнес-логики стоимости и валидации заказов химчистки.
    /// Используется для калькуляции цен со скидками, проверки ограничений и модульного тестирования.
    /// </summary>
    public static class CostCalculationService
    {
        /// <summary>
        /// Расчет итоговой стоимости единицы услуги с учетом скидки (0..100%).
        /// Округление до 2 знаков после запятой по правилам бухгалтерского учета.
        /// </summary>
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

        /// <summary>
        /// Проверка допустимости количества единиц услуги (бизнес-правило: от 1 до 300 шт.).
        /// </summary>
        public static bool ValidateQuantity(int quantity)
        {
            return quantity >= 1 && quantity <= 300;
        }

        /// <summary>
        /// Проверка превышения пороговой стоимости услуги (1000 рублей) для визуального акцентирования.
        /// </summary>
        public static bool IsHighPriceService(decimal price)
        {
            return price >= 1000.00m;
        }

        /// <summary>
        /// Расчет суммарной стоимости позиции в корзине: количество * цена со скидкой.
        /// </summary>
        public static decimal CalculateItemSubtotal(decimal basePrice, int discountPercent, int quantity)
        {
            if (!ValidateQuantity(quantity))
                throw new ArgumentOutOfRangeException(nameof(quantity), "Количество изделий должно быть в диапазоне от 1 до 300 шт.");

            decimal finalUnitPrice = CalculateDiscountedPrice(basePrice, discountPercent);
            return Math.Round(finalUnitPrice * quantity, 2);
        }

        /// <summary>
        /// Расчет общей суммы набора позиций заказа.
        /// </summary>
        public static decimal CalculateOrderTotal(IEnumerable<decimal> itemSubtotals)
        {
            if (itemSubtotals == null)
                return 0m;

            return itemSubtotals.Sum();
        }
    }
}
