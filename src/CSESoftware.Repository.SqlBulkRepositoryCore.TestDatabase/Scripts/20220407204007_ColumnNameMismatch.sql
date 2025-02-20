BEGIN TRANSACTION;
GO

ALTER TABLE [FamilyTrees] ADD [HomeId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
GO

CREATE TABLE [Home_Alpha] (
    [HomeId] uniqueidentifier NOT NULL,
    [Home_Name] nvarchar(max) NULL,
    [Home_Address] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Home_Alpha] PRIMARY KEY ([HomeId])
);
GO

CREATE INDEX [IX_FamilyTrees_HomeId] ON [FamilyTrees] ([HomeId]);
GO

ALTER TABLE [FamilyTrees] ADD CONSTRAINT [FK_FamilyTrees_Home_Alpha_HomeId] FOREIGN KEY ([HomeId]) REFERENCES [Home_Alpha] ([HomeId]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220407204007_ColumnNameMismatch', N'5.0.10');
GO

COMMIT;
GO