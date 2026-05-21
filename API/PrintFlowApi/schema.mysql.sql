CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `InventoryItems` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(140) CHARACTER SET utf8mb4 NOT NULL,
        `Category` varchar(90) CHARACTER SET utf8mb4 NOT NULL,
        `Unit` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `Available` decimal(12,2) NOT NULL,
        `Minimum` decimal(12,2) NOT NULL,
        `Supplier` varchar(140) CHARACTER SET utf8mb4 NOT NULL,
        `UnitCost` decimal(10,2) NOT NULL,
        `Active` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_InventoryItems` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `Products` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Slug` varchar(90) CHARACTER SET utf8mb4 NOT NULL,
        `Name` varchar(140) CHARACTER SET utf8mb4 NOT NULL,
        `Category` varchar(90) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(900) CHARACTER SET utf8mb4 NOT NULL,
        `ImageUrl` varchar(900) CHARACTER SET utf8mb4 NOT NULL,
        `BasePrice` decimal(10,2) NOT NULL,
        `BaseDeadlineDays` int NOT NULL,
        `AllowUpload` tinyint(1) NOT NULL,
        `AllowPickup` tinyint(1) NOT NULL,
        `AllowDelivery` tinyint(1) NOT NULL,
        `AllowPickupPayment` tinyint(1) NOT NULL,
        `RequiresAdvancePayment` tinyint(1) NOT NULL,
        `Active` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_Products` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `SystemSettings` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CompanyName` varchar(140) CHARACTER SET utf8mb4 NOT NULL,
        `CompanyEmail` varchar(180) CHARACTER SET utf8mb4 NOT NULL,
        `CompanyPhone` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `RequireAdminPasswordForSensitiveActions` tinyint(1) NOT NULL,
        `AdminActionPasswordHash` varchar(300) CHARACTER SET utf8mb4 NULL,
        `AutoStockDeductionEnabled` tinyint(1) NOT NULL,
        `StockDeductionTriggerStatus` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_SystemSettings` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `Users` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(140) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(180) CHARACTER SET utf8mb4 NOT NULL,
        `Phone` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `Document` varchar(30) CHARACTER SET utf8mb4 NULL,
        `Address` varchar(260) CHARACTER SET utf8mb4 NULL,
        `ContactPreference` varchar(40) CHARACTER SET utf8mb4 NULL,
        `PasswordHash` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `Role` int NOT NULL,
        `Active` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `StockMovements` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `InventoryItemId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Type` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `Quantity` decimal(12,2) NOT NULL,
        `Reason` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `OrderId` char(36) COLLATE ascii_general_ci NULL,
        `CreatedById` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_StockMovements` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_StockMovements_InventoryItems_InventoryItemId` FOREIGN KEY (`InventoryItemId`) REFERENCES `InventoryItems` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `ProductOptions` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Type` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
        `Name` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `PriceDelta` decimal(10,2) NOT NULL,
        `DeadlineDeltaDays` int NOT NULL,
        CONSTRAINT `PK_ProductOptions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ProductOptions_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `ProductQuantities` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Quantity` int NOT NULL,
        CONSTRAINT `PK_ProductQuantities` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ProductQuantities_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `Orders` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Number` varchar(24) CHARACTER SET utf8mb4 NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Quantity` int NOT NULL,
        `Size` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Material` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `PrintMode` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Finishing` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Urgency` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `DeliveryMode` int NOT NULL,
        `PaymentMethod` int NOT NULL,
        `PaymentStatus` int NOT NULL,
        `Status` int NOT NULL,
        `Subtotal` decimal(10,2) NOT NULL,
        `UrgencyFee` decimal(10,2) NOT NULL,
        `DeliveryFee` decimal(10,2) NOT NULL,
        `Total` decimal(10,2) NOT NULL,
        `EstimatedDays` int NOT NULL,
        `Deadline` datetime(6) NULL,
        `Notes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `Owner` varchar(120) CHARACTER SET utf8mb4 NULL,
        `Priority` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_Orders` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Orders_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_Orders_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `PasswordResetTokens` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TokenHash` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
        `ExpiresAt` datetime(6) NOT NULL,
        `UsedAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `IpAddress` varchar(80) CHARACTER SET utf8mb4 NULL,
        `UserAgent` varchar(400) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_PasswordResetTokens` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PasswordResetTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `Quotes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Number` varchar(24) CHARACTER SET utf8mb4 NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProductId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Quantity` int NOT NULL,
        `Size` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Material` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `PrintMode` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Finishing` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `Urgency` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `DeliveryMode` int NOT NULL,
        `Status` int NOT NULL,
        `Subtotal` decimal(10,2) NOT NULL,
        `UrgencyFee` decimal(10,2) NOT NULL,
        `DeliveryFee` decimal(10,2) NOT NULL,
        `Total` decimal(10,2) NOT NULL,
        `EstimatedDays` int NOT NULL,
        `Notes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `ConvertedOrderId` char(36) COLLATE ascii_general_ci NULL,
        `ExpiresAt` datetime(6) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_Quotes` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Quotes_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_Quotes_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `OrderFiles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `OrderId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FileName` varchar(260) CHARACTER SET utf8mb4 NOT NULL,
        `StorageUrl` varchar(900) CHARACTER SET utf8mb4 NULL,
        `UploadedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_OrderFiles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_OrderFiles_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `OrderHistories` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `OrderId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Status` varchar(160) CHARACTER SET utf8mb4 NOT NULL,
        `Notes` varchar(700) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_OrderHistories` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_OrderHistories_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE TABLE `Payments` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `OrderId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Method` int NOT NULL,
        `Status` int NOT NULL,
        `Amount` decimal(10,2) NOT NULL,
        `Provider` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
        `ProviderReference` varchar(120) CHARACTER SET utf8mb4 NULL,
        `TransactionId` varchar(120) CHARACTER SET utf8mb4 NULL,
        `ReceiptUrl` varchar(900) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        `PaidAt` datetime(6) NULL,
        CONSTRAINT `PK_Payments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Payments_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_OrderFiles_OrderId` ON `OrderFiles` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_OrderHistories_OrderId` ON `OrderHistories` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE UNIQUE INDEX `IX_Orders_Number` ON `Orders` (`Number`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_Orders_ProductId` ON `Orders` (`ProductId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_Orders_UserId` ON `Orders` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_PasswordResetTokens_TokenHash` ON `PasswordResetTokens` (`TokenHash`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_PasswordResetTokens_UserId` ON `PasswordResetTokens` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE UNIQUE INDEX `IX_Payments_OrderId` ON `Payments` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_Payments_ProviderReference` ON `Payments` (`ProviderReference`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_ProductOptions_ProductId` ON `ProductOptions` (`ProductId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_ProductQuantities_ProductId` ON `ProductQuantities` (`ProductId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE UNIQUE INDEX `IX_Products_Slug` ON `Products` (`Slug`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE UNIQUE INDEX `IX_Quotes_Number` ON `Quotes` (`Number`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_Quotes_ProductId` ON `Quotes` (`ProductId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_Quotes_UserId` ON `Quotes` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE INDEX `IX_StockMovements_InventoryItemId` ON `StockMovements` (`InventoryItemId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260520163902_InitialOperationalSchema') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260520163902_InitialOperationalSchema', '9.0.16');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260521124827_CustomerSelfServiceSettings') THEN

    ALTER TABLE `SystemSettings` ADD `AllowCustomerOrderCancellation` tinyint(1) NOT NULL DEFAULT FALSE;
    ALTER TABLE `SystemSettings` ADD `AllowCustomerOrderEdit` tinyint(1) NOT NULL DEFAULT FALSE;
    ALTER TABLE `SystemSettings` ADD `AllowCustomerQuoteEdit` tinyint(1) NOT NULL DEFAULT FALSE;
    ALTER TABLE `SystemSettings` ADD `AllowCustomerRefundRequest` tinyint(1) NOT NULL DEFAULT FALSE;

    UPDATE SystemSettings
    SET AllowCustomerQuoteEdit = 1,
        AllowCustomerOrderCancellation = 1;

    UPDATE Orders
    SET PaymentStatus = 2,
        Status = 2
    WHERE PaymentMethod = 2
      AND PaymentStatus = 1;

    UPDATE Payments
    SET Status = 2,
        PaidAt = COALESCE(PaidAt, UTC_TIMESTAMP()),
        UpdatedAt = UTC_TIMESTAMP()
    WHERE Method = 2
      AND Status = 1;

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260521124827_CustomerSelfServiceSettings', '9.0.16');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;
