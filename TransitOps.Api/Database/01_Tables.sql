-- 01_Tables.sql: TransitOps Database Tables & Schema
USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'TransitOpsDb')
BEGIN
    CREATE DATABASE TransitOpsDb;
END
GO

USE TransitOpsDb;
GO

-- 1. Roles table
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_Name UNIQUE
    );

    INSERT INTO dbo.Roles (Name) VALUES 
        ('FleetManager'),
        ('Driver'),
        ('SafetyOfficer'),
        ('FinancialAnalyst');
END
GO

-- 2. Users table
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Email NVARCHAR(150) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        RoleId INT NOT NULL CONSTRAINT FK_Users_Roles FOREIGN KEY REFERENCES dbo.Roles(Id),
        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

-- 3. Vehicles table
IF OBJECT_ID('dbo.Vehicles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vehicles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RegistrationNumber NVARCHAR(50) NOT NULL CONSTRAINT UQ_Vehicles_RegistrationNumber UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        Region NVARCHAR(50) NOT NULL,
        MaxLoadKg DECIMAL(18,2) NOT NULL CONSTRAINT CK_Vehicles_MaxLoadKg CHECK (MaxLoadKg > 0),
        Odometer DECIMAL(18,2) NOT NULL CONSTRAINT CK_Vehicles_Odometer CHECK (Odometer >= 0),
        AcquisitionCost DECIMAL(18,2) NOT NULL CONSTRAINT CK_Vehicles_AcquisitionCost CHECK (AcquisitionCost > 0),
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Vehicles_Status DEFAULT ('Available')
            CONSTRAINT CK_Vehicles_Status CHECK (Status IN ('Available','OnTrip','InShop','Retired')),
        RowVer ROWVERSION NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Vehicles_Status_Type_Region ON dbo.Vehicles(Status, Type, Region);
END
GO

-- 4. Drivers table
IF OBJECT_ID('dbo.Drivers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Drivers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        LicenseNumber NVARCHAR(50) NOT NULL CONSTRAINT UQ_Drivers_LicenseNumber UNIQUE,
        LicenseCategory NVARCHAR(20) NOT NULL,
        LicenseExpiry DATE NOT NULL,
        ContactNumber NVARCHAR(30) NOT NULL,
        SafetyScore INT NOT NULL CONSTRAINT CK_Drivers_SafetyScore CHECK (SafetyScore BETWEEN 0 AND 100),
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Drivers_Status DEFAULT ('Available')
            CONSTRAINT CK_Drivers_Status CHECK (Status IN ('Available','OnTrip','OffDuty','Suspended')),
        RowVer ROWVERSION NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Drivers_Status_Expiry ON dbo.Drivers(Status, LicenseExpiry);
END
GO

-- 5. Trips table
IF OBJECT_ID('dbo.Trips', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Trips (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Source NVARCHAR(150) NOT NULL,
        Destination NVARCHAR(150) NOT NULL,
        VehicleId INT NOT NULL CONSTRAINT FK_Trips_Vehicles FOREIGN KEY REFERENCES dbo.Vehicles(Id),
        DriverId INT NOT NULL CONSTRAINT FK_Trips_Drivers FOREIGN KEY REFERENCES dbo.Drivers(Id),
        CargoWeightKg DECIMAL(18,2) NOT NULL CONSTRAINT CK_Trips_CargoWeight CHECK (CargoWeightKg > 0),
        PlannedDistanceKm DECIMAL(18,2) NOT NULL CONSTRAINT CK_Trips_PlannedDistance CHECK (PlannedDistanceKm > 0),
        Revenue DECIMAL(18,2) NOT NULL CONSTRAINT DF_Trips_Revenue DEFAULT (0) CONSTRAINT CK_Trips_Revenue CHECK (Revenue >= 0),
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Trips_Status DEFAULT ('Draft')
            CONSTRAINT CK_Trips_Status CHECK (Status IN ('Draft','Dispatched','Completed','Cancelled')),
        FinalOdometer DECIMAL(18,2) NULL,
        FuelConsumedL DECIMAL(18,2) NULL,
        ActualDistanceKm DECIMAL(18,2) NULL,
        CreatedBy INT NOT NULL CONSTRAINT FK_Trips_Users FOREIGN KEY REFERENCES dbo.Users(Id),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Trips_CreatedAt DEFAULT (SYSUTCDATETIME()),
        DispatchedAt DATETIME2 NULL,
        CompletedAt DATETIME2 NULL,
        RowVer ROWVERSION NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Trips_Status ON dbo.Trips(Status);
    CREATE NONCLUSTERED INDEX IX_Trips_VehicleId ON dbo.Trips(VehicleId);
    CREATE NONCLUSTERED INDEX IX_Trips_DriverId ON dbo.Trips(DriverId);

    -- DB-level race backstop: Filtered unique indexes
    CREATE UNIQUE NONCLUSTERED INDEX UX_Trips_Vehicle_Dispatched 
        ON dbo.Trips(VehicleId) 
        WHERE Status = 'Dispatched';

    CREATE UNIQUE NONCLUSTERED INDEX UX_Trips_Driver_Dispatched 
        ON dbo.Trips(DriverId) 
        WHERE Status = 'Dispatched';
END
GO

-- 6. MaintenanceLogs table
IF OBJECT_ID('dbo.MaintenanceLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MaintenanceLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        VehicleId INT NOT NULL CONSTRAINT FK_MaintenanceLogs_Vehicles FOREIGN KEY REFERENCES dbo.Vehicles(Id),
        Description NVARCHAR(255) NOT NULL,
        Cost DECIMAL(18,2) NOT NULL CONSTRAINT CK_MaintenanceLogs_Cost CHECK (Cost >= 0),
        StartDate DATETIME2 NOT NULL CONSTRAINT DF_MaintenanceLogs_StartDate DEFAULT (SYSUTCDATETIME()),
        EndDate DATETIME2 NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_MaintenanceLogs_Status DEFAULT ('Active')
            CONSTRAINT CK_MaintenanceLogs_Status CHECK (Status IN ('Active','Closed'))
    );

    CREATE NONCLUSTERED INDEX IX_MaintenanceLogs_VehicleId ON dbo.MaintenanceLogs(VehicleId);

    -- Filtered unique index: Only one Active maintenance log per vehicle
    CREATE UNIQUE NONCLUSTERED INDEX UX_MaintenanceLogs_Vehicle_Active 
        ON dbo.MaintenanceLogs(VehicleId) 
        WHERE Status = 'Active';
END
GO

-- 7. FuelLogs table
IF OBJECT_ID('dbo.FuelLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FuelLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        VehicleId INT NOT NULL CONSTRAINT FK_FuelLogs_Vehicles FOREIGN KEY REFERENCES dbo.Vehicles(Id),
        TripId INT NULL CONSTRAINT FK_FuelLogs_Trips FOREIGN KEY REFERENCES dbo.Trips(Id),
        Liters DECIMAL(18,2) NOT NULL CONSTRAINT CK_FuelLogs_Liters CHECK (Liters > 0),
        Cost DECIMAL(18,2) NOT NULL CONSTRAINT CK_FuelLogs_Cost CHECK (Cost >= 0),
        LogDate DATETIME2 NOT NULL CONSTRAINT DF_FuelLogs_LogDate DEFAULT (SYSUTCDATETIME())
    );

    CREATE NONCLUSTERED INDEX IX_FuelLogs_VehicleId_LogDate ON dbo.FuelLogs(VehicleId, LogDate);
END
GO

-- 8. Expenses table
IF OBJECT_ID('dbo.Expenses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Expenses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        VehicleId INT NOT NULL CONSTRAINT FK_Expenses_Vehicles FOREIGN KEY REFERENCES dbo.Vehicles(Id),
        TripId INT NULL CONSTRAINT FK_Expenses_Trips FOREIGN KEY REFERENCES dbo.Trips(Id),
        Type NVARCHAR(30) NOT NULL CONSTRAINT CK_Expenses_Type CHECK (Type IN ('Toll','Maintenance','Other')),
        Amount DECIMAL(18,2) NOT NULL CONSTRAINT CK_Expenses_Amount CHECK (Amount > 0),
        ExpenseDate DATETIME2 NOT NULL CONSTRAINT DF_Expenses_ExpenseDate DEFAULT (SYSUTCDATETIME()),
        Notes NVARCHAR(500) NULL
    );

    CREATE NONCLUSTERED INDEX IX_Expenses_VehicleId_ExpenseDate ON dbo.Expenses(VehicleId, ExpenseDate);
END
GO

