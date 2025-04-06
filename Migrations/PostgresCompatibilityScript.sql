-- PostgreSQL compatibility script for QuanVitLonManager
-- Script initialization
SELECT current_timestamp AS "Script Started";

BEGIN;

-- Create migrations history table if it doesn't exist
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" text NOT NULL,
    "ProductVersion" text NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Log progress
DO $$ 
BEGIN
    RAISE NOTICE 'Created __EFMigrationsHistory table';
END $$;

-- Create identity tables with PostgreSQL data types
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
    "Id" text NOT NULL,
    "Name" text NULL,
    "NormalizedName" text NULL,
    "ConcurrencyStamp" text NULL,
    CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id" text NOT NULL,
    "UserName" text NULL,
    "NormalizedUserName" text NULL,
    "Email" text NULL,
    "NormalizedEmail" text NULL,
    "EmailConfirmed" boolean NOT NULL DEFAULT false,
    "PasswordHash" text NULL,
    "SecurityStamp" text NULL,
    "ConcurrencyStamp" text NULL,
    "PhoneNumber" text NULL,
    "PhoneNumberConfirmed" boolean NOT NULL DEFAULT false,
    "TwoFactorEnabled" boolean NOT NULL DEFAULT false,
    "LockoutEnd" timestamp NULL,
    "LockoutEnabled" boolean NOT NULL DEFAULT false,
    "AccessFailedCount" integer NOT NULL DEFAULT 0,
    "FullName" text NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
);

-- Log progress
DO $$ 
BEGIN
    RAISE NOTICE 'Created identity tables';
END $$;

CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
    "Id" serial NOT NULL,
    "RoleId" text NOT NULL,
    "ClaimType" text NULL,
    "ClaimValue" text NULL,
    CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
    "Id" serial NOT NULL,
    "UserId" text NOT NULL,
    "ClaimType" text NULL,
    "ClaimValue" text NULL,
    CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text NULL,
    "UserId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
    "UserId" text NOT NULL,
    "RoleId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
    "UserId" text NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- Log progress
DO $$ 
BEGIN
    RAISE NOTICE 'Created identity relation tables';
END $$;

-- Create application tables
CREATE TABLE IF NOT EXISTS "Categories" (
    "Id" serial NOT NULL,
    "Name" text NOT NULL,
    "Description" text NULL,
    "ImageUrl" text NULL,
    "DisplayOrder" integer NOT NULL DEFAULT 0,
    CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
);

-- Log progress
DO $$ 
BEGIN
    RAISE NOTICE 'Created Categories table';
END $$;

CREATE TABLE IF NOT EXISTS "MenuItems" (
    "Id" serial NOT NULL,
    "Name" text NOT NULL,
    "Description" text NULL,
    "Price" numeric(18,2) NOT NULL,
    "OriginalPrice" numeric(18,2) NOT NULL DEFAULT 0,
    "ImageUrl" text NULL,
    "IsAvailable" boolean NOT NULL DEFAULT true,
    "IsFeatured" boolean NOT NULL DEFAULT false,
    "CategoryId" integer NOT NULL,
    "IsPopular" boolean NOT NULL DEFAULT false,
    "IsNew" boolean NOT NULL DEFAULT false,
    "IsDiscount" boolean NOT NULL DEFAULT false,
    "NutritionInfo" text NULL,
    CONSTRAINT "PK_MenuItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MenuItems_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT
);

-- Log progress
DO $$ 
BEGIN
    RAISE NOTICE 'Created MenuItems table';
END $$;

CREATE TABLE IF NOT EXISTS "Tables" (
    "Id" serial NOT NULL,
    "TableNumber" text NOT NULL,
    "IsAvailable" boolean NOT NULL DEFAULT true,
    "Area" text NOT NULL,
    "MaxCapacity" integer NOT NULL DEFAULT 4,
    CONSTRAINT "PK_Tables" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Orders" (
    "Id" serial NOT NULL,
    "UserId" text NULL,
    "OrderDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "TotalAmount" numeric(18,2) NOT NULL,
    "Status" text NOT NULL,
    "PaymentMethod" text NULL,
    "TableNumber" text NULL,
    "BillingStatus" text NULL,
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Orders_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

-- Log progress
DO $$ 
BEGIN
    RAISE NOTICE 'Created Tables and Orders tables';
END $$;

CREATE TABLE IF NOT EXISTS "Reservations" (
    "Id" serial NOT NULL,
    "UserId" text NOT NULL,
    "TableId" integer NOT NULL,
    "ReservationDate" timestamp NOT NULL,
    "Duration" integer NOT NULL DEFAULT 60,
    "Status" text NOT NULL,
    "NumberOfGuests" integer NOT NULL DEFAULT 1,
    "Notes" text NOT NULL DEFAULT '',
    CONSTRAINT "PK_Reservations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Reservations_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Reservations_Tables_TableId" FOREIGN KEY ("TableId") REFERENCES "Tables" ("Id") ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS "CartItems" (
    "Id" serial NOT NULL,
    "MenuItemId" integer NOT NULL,
    "Quantity" integer NOT NULL DEFAULT 1,
    "Price" numeric(18,2) NOT NULL,
    "Notes" text NULL,
    "DateAdded" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UserId" text NOT NULL,
    CONSTRAINT "PK_CartItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CartItems_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CartItems_MenuItems_MenuItemId" FOREIGN KEY ("MenuItemId") REFERENCES "MenuItems" ("Id") ON DELETE CASCADE
);

-- Log progress
DO $$ 
BEGIN
    RAISE NOTICE 'Created Reservations and CartItems tables';
END $$;

CREATE TABLE IF NOT EXISTS "OrderDetails" (
    "Id" serial NOT NULL,
    "OrderId" integer NOT NULL,
    "MenuItemId" integer NOT NULL,
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric(18,2) NOT NULL,
    "Subtotal" numeric(18,2) NOT NULL,
    "Notes" text NULL,
    CONSTRAINT "PK_OrderDetails" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OrderDetails_MenuItems_MenuItemId" FOREIGN KEY ("MenuItemId") REFERENCES "MenuItems" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OrderDetails_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "DishOrders" (
    "Id" serial NOT NULL,
    "OrderId" integer NOT NULL,
    "DishName" text NOT NULL,
    "Quantity" integer NOT NULL,
    "Price" numeric(18,2) NOT NULL,
    "TotalPrice" numeric(18,2) NOT NULL,
    "Status" text NOT NULL,
    "Notes" text NULL,
    CONSTRAINT "PK_DishOrders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DishOrders_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
);

-- Log progress
DO $$ 
BEGIN
    RAISE NOTICE 'Created OrderDetails and DishOrders tables';
END $$;

CREATE TABLE IF NOT EXISTS "Reviews" (
    "Id" serial NOT NULL,
    "MenuItemId" integer NOT NULL,
    "UserId" text NOT NULL,
    "Rating" integer NOT NULL,
    "Comment" text NULL,
    "ReviewDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "PK_Reviews" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Reviews_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reviews_MenuItems_MenuItemId" FOREIGN KEY ("MenuItemId") REFERENCES "MenuItems" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "RestaurantInfo" (
    "Id" serial NOT NULL,
    "Name" text NOT NULL,
    "Address" text NOT NULL,
    "PhoneNumber" text NOT NULL,
    "Email" text NULL,
    "LogoUrl" text NULL,
    "OpeningHours" text NULL,
    CONSTRAINT "PK_RestaurantInfo" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "UserCarts" (
    "Id" serial NOT NULL,
    "UserId" text NOT NULL,
    "CreatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "PK_UserCarts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserCarts_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- Log progress
DO $$ 
BEGIN
    RAISE NOTICE 'Created remaining tables';
END $$;

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" ("NormalizedName") WHERE "NormalizedName" IS NOT NULL;
CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName") WHERE "NormalizedUserName" IS NOT NULL;
CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
CREATE INDEX IF NOT EXISTS "IX_CartItems_MenuItemId" ON "CartItems" ("MenuItemId");
CREATE INDEX IF NOT EXISTS "IX_CartItems_UserId" ON "CartItems" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_DishOrders_OrderId" ON "DishOrders" ("OrderId");
CREATE INDEX IF NOT EXISTS "IX_MenuItems_CategoryId" ON "MenuItems" ("CategoryId");
CREATE INDEX IF NOT EXISTS "IX_OrderDetails_MenuItemId" ON "OrderDetails" ("MenuItemId");
CREATE INDEX IF NOT EXISTS "IX_OrderDetails_OrderId" ON "OrderDetails" ("OrderId");
CREATE INDEX IF NOT EXISTS "IX_Orders_UserId" ON "Orders" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_Reservations_TableId" ON "Reservations" ("TableId");
CREATE INDEX IF NOT EXISTS "IX_Reservations_UserId" ON "Reservations" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_Reviews_MenuItemId" ON "Reviews" ("MenuItemId");
CREATE INDEX IF NOT EXISTS "IX_Reviews_UserId" ON "Reviews" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserCarts_UserId" ON "UserCarts" ("UserId");

-- Log progress
DO $$ 
BEGIN
    RAISE NOTICE 'Created all indexes';
END $$;

-- Create initial category data
INSERT INTO "Categories" ("Name", "Description", "DisplayOrder")
VALUES 
('Món chính', 'Các món chính trong thực đơn', 1),
('Món phụ', 'Các món ăn kèm', 2), 
('Đồ uống', 'Các loại đồ uống', 3),
('Tráng miệng', 'Các món tráng miệng', 4)
ON CONFLICT DO NOTHING;

-- Add bootstrap data
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250406035817_FixPostgreSqlTypes', '8.0.3')
ON CONFLICT ("MigrationId") DO NOTHING;

-- Log progress
DO $$ 
BEGIN
    RAISE NOTICE 'Inserted initial data';
END $$;

COMMIT;

-- Script completion
SELECT current_timestamp AS "Script Completed";

-- Log final status
DO $$ 
BEGIN
    RAISE NOTICE 'PostgreSQL setup script completed successfully';
END $$; 