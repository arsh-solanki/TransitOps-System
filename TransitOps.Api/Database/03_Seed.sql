-- 03_Seed.sql: Optional manual seed script for TransitOps database
USE TransitOpsDb;
GO

-- Ensure Roles exist
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'FleetManager')
    INSERT INTO dbo.Roles (Name) VALUES ('FleetManager');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'Driver')
    INSERT INTO dbo.Roles (Name) VALUES ('Driver');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'SafetyOfficer')
    INSERT INTO dbo.Roles (Name) VALUES ('SafetyOfficer');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'FinancialAnalyst')
    INSERT INTO dbo.Roles (Name) VALUES ('FinancialAnalyst');
GO

-- Demo Users will also be automatically seeded on API startup via DatabaseSeeder.cs if empty.

