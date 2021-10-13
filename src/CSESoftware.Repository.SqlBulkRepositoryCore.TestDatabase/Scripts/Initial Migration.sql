IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [FamilyTrees] (
    [Id] uniqueidentifier NOT NULL,
    [IsAlive] bit NOT NULL,
    [Gender] nvarchar(max) NULL,
    [Birthdate] datetime2 NOT NULL,
    [FatherId] uniqueidentifier NOT NULL,
    [MotherId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_FamilyTrees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FamilyTrees_FamilyTrees_FatherId] FOREIGN KEY ([FatherId]) REFERENCES [FamilyTrees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_FamilyTrees_FamilyTrees_MotherId] FOREIGN KEY ([MotherId]) REFERENCES [FamilyTrees] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [FamilyTreeLink] (
    [PrimarySiblingId] uniqueidentifier NOT NULL,
    [SecondarySiblingId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_FamilyTreeLink] PRIMARY KEY ([PrimarySiblingId], [SecondarySiblingId]),
    CONSTRAINT [FK_FamilyTreeLink_FamilyTrees_PrimarySiblingId] FOREIGN KEY ([PrimarySiblingId]) REFERENCES [FamilyTrees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_FamilyTreeLink_FamilyTrees_SecondarySiblingId] FOREIGN KEY ([SecondarySiblingId]) REFERENCES [FamilyTrees] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_FamilyTreeLink_SecondarySiblingId] ON [FamilyTreeLink] ([SecondarySiblingId]);
GO

CREATE INDEX [IX_FamilyTrees_FatherId] ON [FamilyTrees] ([FatherId]);
GO

CREATE INDEX [IX_FamilyTrees_MotherId] ON [FamilyTrees] ([MotherId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211009151942_InitialMigration', N'5.0.10');
GO

COMMIT;
GO

