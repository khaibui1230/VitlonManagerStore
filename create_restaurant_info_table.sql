IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RestaurantInfo')
BEGIN
    CREATE TABLE [RestaurantInfo] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Address] nvarchar(255) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [TaxId] nvarchar(20) NOT NULL,
        [LogoUrl] nvarchar(255) NOT NULL,
        [WelcomeMessage] nvarchar(255) NOT NULL,
        [GoodbyeMessage] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_RestaurantInfo] PRIMARY KEY ([Id])
    );
END; 