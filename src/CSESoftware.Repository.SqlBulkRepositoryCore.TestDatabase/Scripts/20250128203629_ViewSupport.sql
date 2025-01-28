BEGIN TRANSACTION;

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[FamilyHomeView]
AS
SELECT fam.Gender, hom.Home_Name, hom.Home_Address
FROM   dbo.FamilyTrees AS fam INNER JOIN
             dbo.Home_Alpha AS hom ON fam.HomeId = hom.HomeId
WHERE (fam.IsAlive = 1)
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250128203629_ViewSupport', N'9.0.1');

COMMIT;
GO
