-- ===================================================================
-- Скрипт создания базы данных и наполнения тестовыми данными
-- База данных: himchistka
-- Проект: Himchistka (WPF + Entity Framework 6)
-- ===================================================================

USE [master];
GO

-- 1. Создание базы данных (если не существует) без жестких путей к файлам
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'himchistka')
BEGIN
    CREATE DATABASE [himchistka];
    PRINT N'База данных [himchistka] успешно создана.';
END
ELSE
BEGIN
    PRINT N'База данных [himchistka] уже существует.';
END
GO

USE [himchistka];
GO

-- 2. Создание таблиц (если их нет)
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Таблица: role
IF OBJECT_ID(N'[dbo].[role]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[role](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [role] [nvarchar](50) NULL,
        CONSTRAINT [PK_role] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END
GO

-- Таблица: status
IF OBJECT_ID(N'[dbo].[status]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[status](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [status] [nvarchar](50) NULL,
        CONSTRAINT [PK_status] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END
GO

-- Таблица: unit
IF OBJECT_ID(N'[dbo].[unit]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[unit](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [unit] [nvarchar](50) NULL,
        CONSTRAINT [PK_unit] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END
GO

-- Таблица: category
IF OBJECT_ID(N'[dbo].[category]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[category](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [category] [nvarchar](50) NULL,
        CONSTRAINT [PK_category] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END
GO

-- Таблица: material
IF OBJECT_ID(N'[dbo].[material]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[material](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [material] [nvarchar](50) NULL,
        CONSTRAINT [PK_material] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END
GO

-- Таблица: user
IF OBJECT_ID(N'[dbo].[user]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[user](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [email] [nvarchar](50) NULL,
        [phoneNumber] [nvarchar](25) NULL,
        [password] [nvarchar](50) NULL,
        [roleId] [int] NOT NULL,
        [surname] [nvarchar](50) NULL,
        [name] [nvarchar](50) NOT NULL,
        [patronymic] [nvarchar](50) NULL,
        CONSTRAINT [PK_user] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_user_role] FOREIGN KEY([roleId]) REFERENCES [dbo].[role] ([id])
    );
END
GO

-- Таблица: product
IF OBJECT_ID(N'[dbo].[product]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[product](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [name] [nvarchar](50) NOT NULL,
        [price] [int] NOT NULL,
        [discount] [int] NULL,
        [image] [nvarchar](50) NULL,
        [amount] [nvarchar](50) NULL,
        [categoryId] [int] NOT NULL,
        [materialId] [int] NOT NULL,
        CONSTRAINT [PK_product] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_product_category] FOREIGN KEY([categoryId]) REFERENCES [dbo].[category] ([id]),
        CONSTRAINT [FK_product_material] FOREIGN KEY([materialId]) REFERENCES [dbo].[material] ([id])
    );
END
GO

-- Таблица: order
IF OBJECT_ID(N'[dbo].[order]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[order](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [userid] [int] NOT NULL,
        [dateOfCart] [datetime] NULL,
        [dateOfBuy] [datetime] NULL,
        CONSTRAINT [PK_order] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_order_user] FOREIGN KEY([userid]) REFERENCES [dbo].[user] ([id])
    );
END
GO

-- Таблица: productOrder
IF OBJECT_ID(N'[dbo].[productOrder]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[productOrder](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [productId] [int] NOT NULL,
        [orderId] [int] NOT NULL,
        [statusId] [int] NOT NULL,
        [unitId] [int] NULL,
        CONSTRAINT [PK_productOrder] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_productOrder_order] FOREIGN KEY([orderId]) REFERENCES [dbo].[order] ([id]),
        CONSTRAINT [FK_productOrder_product] FOREIGN KEY([productId]) REFERENCES [dbo].[product] ([id]),
        CONSTRAINT [FK_productOrder_status] FOREIGN KEY([statusId]) REFERENCES [dbo].[status] ([id]),
        CONSTRAINT [FK_productOrder_unit] FOREIGN KEY([unitId]) REFERENCES [dbo].[unit] ([id])
    );
END
GO

-- ===================================================================
-- 3. Очистка старых данных (если скрипт запускается повторно)
-- ===================================================================
PRINT N'Очистка существующих данных...';

-- Отключаем внешние ключи на время очистки
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";

DELETE FROM [dbo].[productOrder];
DELETE FROM [dbo].[order];
DELETE FROM [dbo].[product];
DELETE FROM [dbo].[user];
DELETE FROM [dbo].[role];
DELETE FROM [dbo].[status];
DELETE FROM [dbo].[unit];
DELETE FROM [dbo].[category];
DELETE FROM [dbo].[material];

-- Включаем внешние ключи обратно
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
GO

-- ===================================================================
-- 4. Заполнение справочников и базовых данных
-- ===================================================================

PRINT N'Заполнение справочников...';

-- Роли пользователей
SET IDENTITY_INSERT [dbo].[role] ON;
INSERT INTO [dbo].[role] ([id], [role]) VALUES
(1, N'Администратор'),
(2, N'Менеджер'),
(3, N'Клиент');
SET IDENTITY_INSERT [dbo].[role] OFF;
GO

-- Статусы заказов
SET IDENTITY_INSERT [dbo].[status] ON;
INSERT INTO [dbo].[status] ([id], [status]) VALUES
(1, N'Новый'),
(2, N'В обработке'),
(3, N'Готов к выдаче'),
(4, N'Выдан'),
(5, N'Отменен');
SET IDENTITY_INSERT [dbo].[status] OFF;
GO

-- Единицы измерения
SET IDENTITY_INSERT [dbo].[unit] ON;
INSERT INTO [dbo].[unit] ([id], [unit]) VALUES
(1, N'шт.'),
(2, N'кг'),
(3, N'кв. м'),
(4, N'компл.'),
(5, N'пара');
SET IDENTITY_INSERT [dbo].[unit] OFF;
GO

-- Категории изделий / услуг
SET IDENTITY_INSERT [dbo].[category] ON;
INSERT INTO [dbo].[category] ([id], [category]) VALUES
(1, N'Верхняя одежда'),
(2, N'Повседневная одежда'),
(3, N'Текстиль и шторы'),
(4, N'Кожа и мех'),
(5, N'Обувь и сумки'),
(6, N'Ковры и покрытия');
SET IDENTITY_INSERT [dbo].[category] OFF;
GO

-- Материалы изделий
SET IDENTITY_INSERT [dbo].[material] ON;
INSERT INTO [dbo].[material] ([id], [material]) VALUES
(1, N'Хлопок / Лён'),
(2, N'Шерсть / Кашемир'),
(3, N'Шёлк'),
(4, N'Синтетика / Полиэстер'),
(5, N'Натуральная кожа'),
(6, N'Замша / Нубук'),
(7, N'Пух / Перо'),
(8, N'Смесовая ткань');
SET IDENTITY_INSERT [dbo].[material] OFF;
GO

-- ===================================================================
-- 5. Заполнение пользователей
-- ===================================================================

PRINT N'Заполнение пользователей...';

SET IDENTITY_INSERT [dbo].[user] ON;
INSERT INTO [dbo].[user] ([id], [email], [phoneNumber], [password], [roleId], [surname], [name], [patronymic]) VALUES
(1, N'admin@himchistka.ru',   N'+7 (900) 111-22-33', N'admin123',  1, N'Иванов',    N'Иван',     N'Иванович'),
(2, N'manager@himchistka.ru', N'+7 (900) 222-33-44', N'manager123',2, N'Петрова',   N'Анна',     N'Сергеевна'),
(3, N'smirnov@mail.ru',       N'+7 (911) 333-44-55', N'client123', 3, N'Смирнов',   N'Алексей',  N'Дмитриевич'),
(4, N'kuznetsova@gmail.com',   N'+7 (921) 444-55-66', N'client456', 3, N'Кузнецова', N'Елена',    N'Павловна'),
(5, N'vasiliev@yandex.ru',     N'+7 (931) 555-66-77', N'client789', 3, N'Васильев',  N'Дмитрий',  N'Андреевич');
SET IDENTITY_INSERT [dbo].[user] OFF;
GO

-- ===================================================================
-- 6. Заполнение услуг и товаров (product)
-- ===================================================================

PRINT N'Заполнение услуг химчистки...';

SET IDENTITY_INSERT [dbo].[product] ON;
INSERT INTO [dbo].[product] ([id], [name], [price], [discount], [image], [amount], [categoryId], [materialId]) VALUES
(1,  N'Химчистка пуховика',                  1800, 10, N'puhovik.png',      N'1', 1, 7),
(2,  N'Чистка пальто демисезонного',         2200, 5,  N'palto.png',        N'1', 1, 2),
(3,  N'Чистка кожаной куртки',               3500, 0,  N'kozha_kurtka.png', N'1', 4, 5),
(4,  N'Аквачистка шерстяного костюма',       1700, 15, N'kostyum.png',      N'1', 2, 2),
(5,  N'Стирка и глажка рубашки',             350,  0,  N'rubashka.png',     N'1', 2, 1),
(6,  N'Чистка шелкового платья',             1400, 10, N'dress.png',        N'1', 2, 3),
(7,  N'Химчистка портьерных штор',           450,  0,  N'shtory.png',       N'1', 3, 8),
(8,  N'Стирка тюля',                         250,  0,  N'tyul.png',         N'1', 3, 4),
(9,  N'Реставрация и чистка замшевой обуви', 2400, 20, N'obuv_zamsha.png',  N'1', 5, 6),
(10, N'Чистка шерстяного ковра',             650,  10, N'kover.png',        N'1', 6, 2),
(11, N'Чистка дубленки длинной',             4800, 15, N'dublenka.png',     N'1', 4, 5),
(12, N'Стирка комплекта постельного белья',  800,  0,  N'postelnoe.png',    N'1', 3, 1),
(13, N'Химчистка шубы из норки',             6500, 10, N'shuba.png',        N'1', 4, 5),
(14, N'Химчистка кашемирового свитера',       850,  0,  N'sviter.png',       N'1', 2, 2),
(15, N'Чистка и отпаривание брюк',            600,  5,  N'bruki.png',        N'1', 2, 2),
(16, N'Комплексная чистка кроссовок',         1900, 15, N'krossovki.png',    N'1', 5, 4),
(17, N'Стирка пухового одеяла',              1300, 0,  N'odeyalo.png',      N'1', 3, 7),
(18, N'Чистка пледа из шерсти',               950,  10, N'pled.png',         N'1', 3, 2);
SET IDENTITY_INSERT [dbo].[product] OFF;
GO

-- ===================================================================
-- 7. Заполнение заказов (order)
-- ===================================================================

PRINT N'Заполнение заказов...';

SET IDENTITY_INSERT [dbo].[order] ON;
INSERT INTO [dbo].[order] ([id], [userid], [dateOfCart], [dateOfBuy]) VALUES
(1, 3, '2026-10-15 10:30:00', '2026-10-15 10:45:00'),
(2, 4, '2026-10-20 14:15:00', '2026-10-20 14:30:00'),
(3, 3, '2026-11-01 11:00:00', '2026-11-01 11:20:00'),
(4, 5, '2026-11-10 16:45:00', '2026-11-10 17:00:00'),
(5, 4, '2026-11-12 12:10:00', NULL); -- Корзина в процессе (не оплачена)
SET IDENTITY_INSERT [dbo].[order] OFF;
GO

-- ===================================================================
-- 8. Заполнение позиций заказов (productOrder)
-- ===================================================================

PRINT N'Заполнение позиций заказов...';

SET IDENTITY_INSERT [dbo].[productOrder] ON;
INSERT INTO [dbo].[productOrder] ([id], [productId], [orderId], [statusId], [unitId]) VALUES
-- Заказ 1: Пуховик + Шерстяной костюм (Выдан)
(1, 1,  1, 4, 1),
(2, 4,  1, 4, 1),

-- Заказ 2: Кожаная куртка + Замшевая обувь (Готов к выдаче)
(3, 3,  2, 3, 1),
(4, 9,  2, 3, 5),

-- Заказ 3: Шторы портьерные (В обработке)
(5, 7,  3, 2, 3),

-- Заказ 4: Ковер шерстяной (Новый)
(6, 10, 4, 1, 3),

-- Заказ 5: Текущая корзина клиента (Пальто + Рубашка)
(7, 2,  5, 1, 1),
(8, 5,  5, 1, 1);
SET IDENTITY_INSERT [dbo].[productOrder] OFF;
GO

PRINT N'======================================================';
PRINT N'База данных [himchistka] успешно создана и наполнена!';
PRINT N'======================================================';
GO
