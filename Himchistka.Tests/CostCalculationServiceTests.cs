using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himchistka.AppData;

namespace Himchistka.Tests
{
    /// <summary>
    /// Модульные тесты (Unit Tests) для проверки расчетной бизнес-логики сервиса CostCalculationService.
    /// Соответствует руководству Microsoft Learn по созданию и запуску модульных тестов для управляемого кода.
    /// Паттерн тестирования: AAA (Arrange, Act, Assert).
    /// </summary>
    [TestClass]
    public class CostCalculationServiceTests
    {
        /// <summary>
        /// Тест 1: Проверка корректности расчета стоимости услуги со стандартной скидкой.
        /// Базовая цена: 1200.00 руб., скидка: 15% => Ожидается: 1020.00 руб.
        /// </summary>
        [TestMethod]
        public void CalculateDiscountedPrice_StandardDiscount_ReturnsCorrectDiscountedPrice()
        {
            // Arrange (Подготовка)
            decimal basePrice = 1200.00m;
            int discountPercent = 15;
            decimal expectedPrice = 1020.00m;

            // Act (Действие)
            decimal actualPrice = CostCalculationService.CalculateDiscountedPrice(basePrice, discountPercent);

            // Assert (Проверка)
            Assert.AreEqual(expectedPrice, actualPrice, "Итоговая стоимость со скидкой 15% рассчитана неверно.");
        }

        /// <summary>
        /// Тест 2: Проверка расчета при нулевой скидке (0%).
        /// Базовая цена: 850.00 руб., скидка: 0% => Цена не должна измениться (850.00 руб.).
        /// </summary>
        [TestMethod]
        public void CalculateDiscountedPrice_ZeroDiscount_ReturnsInitialPrice()
        {
            // Arrange
            decimal basePrice = 850.00m;
            int discountPercent = 0;
            decimal expectedPrice = 850.00m;

            // Act
            decimal actualPrice = CostCalculationService.CalculateDiscountedPrice(basePrice, discountPercent);

            // Assert
            Assert.AreEqual(expectedPrice, actualPrice, "При нулевой скидке цена услуги должна оставаться неизменной.");
        }

        /// <summary>
        /// Тест 3: Проверка генерации исключения ArgumentOutOfRangeException при недопустимом значении скидки (>100% или <0%).
        /// </summary>
        [TestMethod]
        public void CalculateDiscountedPrice_InvalidDiscount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            decimal basePrice = 500.00m;
            int invalidDiscountNegative = -5;
            int invalidDiscountExcessive = 110;

            // Act & Assert для отрицательной скидки
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                CostCalculationService.CalculateDiscountedPrice(basePrice, invalidDiscountNegative);
            }, "Метод должен выбрасывать ArgumentOutOfRangeException при отрицательной скидке.");

            // Act & Assert для скидки свыше 100%
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                CostCalculationService.CalculateDiscountedPrice(basePrice, invalidDiscountExcessive);
            }, "Метод должен выбрасывать ArgumentOutOfRangeException при скидке более 100%.");
        }

        /// <summary>
        /// Тест 4: Проверка валидации граничных значений допустимого количества единиц услуги (диапазон 1..300 шт.).
        /// Границы: 0 (ложь), 1 (истина), 300 (истина), 301 (ложь).
        /// </summary>
        [TestMethod]
        public void ValidateQuantity_BoundaryValues_ReturnsExpectedResults()
        {
            // Assert граничных значений
            Assert.IsFalse(CostCalculationService.ValidateQuantity(0), "Значение 0 шт. должно быть недопустимым.");
            Assert.IsTrue(CostCalculationService.ValidateQuantity(1), "Значение 1 шт. должно быть допустимым (нижняя граница).");
            Assert.IsTrue(CostCalculationService.ValidateQuantity(150), "Значение 150 шт. должно быть допустимым (номинальное значение).");
            Assert.IsTrue(CostCalculationService.ValidateQuantity(300), "Значение 300 шт. должно быть допустимым (верхняя граница).");
            Assert.IsFalse(CostCalculationService.ValidateQuantity(301), "Значение 301 шт. должно превышать максимальный лимит.");
        }

        /// <summary>
        /// Тест 5: Проверка порогового условия для визуального цветового акцентирования цен (порог 1000.00 руб.).
        /// </summary>
        [TestMethod]
        public void IsHighPriceService_ThresholdChecks_IdentifiesAccentRequirement()
        {
            // Arrange & Assert
            Assert.IsFalse(CostCalculationService.IsHighPriceService(999.99m), "Цена 999.99 руб. не должна квалифицироваться как высокая.");
            Assert.IsTrue(CostCalculationService.IsHighPriceService(1000.00m), "Цена ровно 1000.00 руб. должна требовать акцентного выделения (порог).");
            Assert.IsTrue(CostCalculationService.IsHighPriceService(2500.00m), "Цена 2500.00 руб. должна требовать акцентного выделения.");
        }
    }
}
