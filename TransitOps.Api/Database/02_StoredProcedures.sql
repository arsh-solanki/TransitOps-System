-- 02_StoredProcedures.sql: Module-wise mode-based stored procedures
USE TransitOpsDb;
GO

-- ============================================================================
-- 1. sp_Auth
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Auth
    @Mode NVARCHAR(30),
    @Email NVARCHAR(150) = NULL,
    @FullName NVARCHAR(150) = NULL,
    @PasswordHash NVARCHAR(255) = NULL,
    @RoleId INT = NULL,
    @UserId INT = NULL,
    @ResultCode INT = 0 OUTPUT,
    @Message NVARCHAR(400) = N'' OUTPUT,
    @NewId INT = 0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @ResultCode = 0;
    SET @Message = N'';
    SET @NewId = 0;

    IF @Mode = N'LOGIN_LOOKUP'
    BEGIN
        SELECT u.Id, u.FullName, u.Email, u.PasswordHash, u.RoleId, r.Name AS RoleName, u.IsActive, u.CreatedAt
        FROM dbo.Users u
        INNER JOIN dbo.Roles r ON u.RoleId = r.Id
        WHERE u.Email = @Email;

        IF @@ROWCOUNT = 0
        BEGIN
            SET @ResultCode = 1;
            SET @Message = N'User not found with the provided email.';
        END
        RETURN;
    END
    ELSE IF @Mode = N'REGISTER'
    BEGIN
        BEGIN TRY
            IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
            BEGIN
                SET @ResultCode = 3;
                SET @Message = N'User with this email already exists.';
                RETURN;
            END

            INSERT INTO dbo.Users (FullName, Email, PasswordHash, RoleId, IsActive, CreatedAt)
            VALUES (@FullName, @Email, @PasswordHash, @RoleId, 1, SYSUTCDATETIME());

            SET @NewId = SCOPE_IDENTITY();
            SET @ResultCode = 0;
            SET @Message = N'User registered successfully.';
        END TRY
        BEGIN CATCH
            SET @ResultCode = 2;
            SET @Message = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END
    ELSE
    BEGIN
        SET @ResultCode = 2;
        SET @Message = N'Invalid mode: ' + ISNULL(@Mode, N'NULL');
        RETURN;
    END
END
GO

-- ============================================================================
-- 2. sp_Vehicle
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Vehicle
    @Mode NVARCHAR(30),
    @Id INT = NULL,
    @RegistrationNumber NVARCHAR(50) = NULL,
    @Name NVARCHAR(100) = NULL,
    @Type NVARCHAR(50) = NULL,
    @Region NVARCHAR(50) = NULL,
    @MaxLoadKg DECIMAL(18,2) = NULL,
    @Odometer DECIMAL(18,2) = NULL,
    @AcquisitionCost DECIMAL(18,2) = NULL,
    @Status NVARCHAR(30) = NULL,
    @Search NVARCHAR(100) = NULL,
    @SortBy NVARCHAR(50) = NULL,
    @SortDir NVARCHAR(10) = NULL,
    @ResultCode INT = 0 OUTPUT,
    @Message NVARCHAR(400) = N'' OUTPUT,
    @NewId INT = 0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @ResultCode = 0;
    SET @Message = N'';
    SET @NewId = 0;

    IF @Mode = N'GET_BY_ID'
    BEGIN
        SELECT Id, RegistrationNumber, Name, Type, Region, MaxLoadKg, Odometer, AcquisitionCost, Status, RowVer
        FROM dbo.Vehicles
        WHERE Id = @Id;

        IF @@ROWCOUNT = 0
        BEGIN
            SET @ResultCode = 1;
            SET @Message = N'Vehicle not found.';
        END
        RETURN;
    END
    ELSE IF @Mode = N'GET_ALL'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX) = N'SELECT Id, RegistrationNumber, Name, Type, Region, MaxLoadKg, Odometer, AcquisitionCost, Status, RowVer FROM dbo.Vehicles WHERE 1=1';
        DECLARE @ParamDef NVARCHAR(MAX) = N'@Type NVARCHAR(50), @Status NVARCHAR(30), @Region NVARCHAR(50), @Search NVARCHAR(100)';

        IF @Type IS NOT NULL
            SET @Sql += N' AND Type = @Type';
        IF @Status IS NOT NULL
            SET @Sql += N' AND Status = @Status';
        IF @Region IS NOT NULL
            SET @Sql += N' AND Region = @Region';
        IF @Search IS NOT NULL AND LEN(TRIM(@Search)) > 0
        BEGIN
            SET @Search = N'%' + TRIM(@Search) + N'%';
            SET @Sql += N' AND (RegistrationNumber LIKE @Search OR Name LIKE @Search)';
        END

        -- Whitelist sort column
        DECLARE @OrderCol NVARCHAR(50) = CASE LOWER(ISNULL(@SortBy, N'id'))
            WHEN N'registrationnumber' THEN N'RegistrationNumber'
            WHEN N'name' THEN N'Name'
            WHEN N'type' THEN N'Type'
            WHEN N'region' THEN N'Region'
            WHEN N'maxloadkg' THEN N'MaxLoadKg'
            WHEN N'odometer' THEN N'Odometer'
            WHEN N'acquisitioncost' THEN N'AcquisitionCost'
            WHEN N'status' THEN N'Status'
            ELSE N'Id'
        END;

        DECLARE @OrderDirection NVARCHAR(4) = CASE UPPER(ISNULL(@SortDir, N'ASC'))
            WHEN N'DESC' THEN N'DESC'
            ELSE N'ASC'
        END;

        SET @Sql += N' ORDER BY ' + @OrderCol + N' ' + @OrderDirection;

        EXEC sp_executesql @Sql, @ParamDef, @Type = @Type, @Status = @Status, @Region = @Region, @Search = @Search;
        RETURN;
    END
    ELSE IF @Mode = N'GET_AVAILABLE'
    BEGIN
        SELECT Id, RegistrationNumber, Name, Type, Region, MaxLoadKg, Odometer, AcquisitionCost, Status, RowVer
        FROM dbo.Vehicles
        WHERE Status = 'Available'
        ORDER BY Name ASC;
        RETURN;
    END
    ELSE IF @Mode = N'INSERT'
    BEGIN
        BEGIN TRY
            IF EXISTS (SELECT 1 FROM dbo.Vehicles WHERE RegistrationNumber = @RegistrationNumber)
            BEGIN
                SET @ResultCode = 3;
                SET @Message = N'A vehicle with registration number ''' + @RegistrationNumber + N''' already exists.';
                RETURN;
            END

            INSERT INTO dbo.Vehicles (RegistrationNumber, Name, Type, Region, MaxLoadKg, Odometer, AcquisitionCost, Status)
            VALUES (@RegistrationNumber, @Name, @Type, @Region, @MaxLoadKg, @Odometer, @AcquisitionCost, ISNULL(@Status, 'Available'));

            SET @NewId = SCOPE_IDENTITY();
            SET @ResultCode = 0;
            SET @Message = N'Vehicle registered successfully.';
        END TRY
        BEGIN CATCH
            IF ERROR_NUMBER() IN (2601, 2627)
            BEGIN
                SET @ResultCode = 3;
                SET @Message = N'A vehicle with registration number ''' + @RegistrationNumber + N''' already exists.';
            END
            ELSE
            BEGIN
                SET @ResultCode = 2;
                SET @Message = ERROR_MESSAGE();
            END
        END CATCH
        RETURN;
    END
    ELSE IF @Mode = N'UPDATE'
    BEGIN
        BEGIN TRY
            IF NOT EXISTS (SELECT 1 FROM dbo.Vehicles WHERE Id = @Id)
            BEGIN
                SET @ResultCode = 1;
                SET @Message = N'Vehicle not found.';
                RETURN;
            END

            IF EXISTS (SELECT 1 FROM dbo.Vehicles WHERE RegistrationNumber = @RegistrationNumber AND Id <> @Id)
            BEGIN
                SET @ResultCode = 3;
                SET @Message = N'A vehicle with registration number ''' + @RegistrationNumber + N''' already exists.';
                RETURN;
            END

            UPDATE dbo.Vehicles
            SET RegistrationNumber = @RegistrationNumber,
                Name = @Name,
                Type = @Type,
                Region = @Region,
                MaxLoadKg = @MaxLoadKg,
                Odometer = @Odometer,
                AcquisitionCost = @AcquisitionCost,
                Status = ISNULL(@Status, Status)
            WHERE Id = @Id;

            SET @ResultCode = 0;
            SET @Message = N'Vehicle updated successfully.';
        END TRY
        BEGIN CATCH
            IF ERROR_NUMBER() IN (2601, 2627)
            BEGIN
                SET @ResultCode = 3;
                SET @Message = N'Duplicate vehicle registration number.';
            END
            ELSE
            BEGIN
                SET @ResultCode = 2;
                SET @Message = ERROR_MESSAGE();
            END
        END CATCH
        RETURN;
    END
    ELSE IF @Mode = N'DELETE'
    BEGIN
        BEGIN TRY
            IF NOT EXISTS (SELECT 1 FROM dbo.Vehicles WHERE Id = @Id)
            BEGIN
                SET @ResultCode = 1;
                SET @Message = N'Vehicle not found.';
                RETURN;
            END

            -- Check if vehicle has trips
            IF EXISTS (SELECT 1 FROM dbo.Trips WHERE VehicleId = @Id)
            BEGIN
                SET @ResultCode = 2;
                SET @Message = N'Cannot delete vehicle with historical trips. Set status to Retired instead.';
                RETURN;
            END

            DELETE FROM dbo.Vehicles WHERE Id = @Id;
            SET @ResultCode = 0;
            SET @Message = N'Vehicle deleted successfully.';
        END TRY
        BEGIN CATCH
            SET @ResultCode = 2;
            SET @Message = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END
    ELSE
    BEGIN
        SET @ResultCode = 2;
        SET @Message = N'Invalid mode: ' + ISNULL(@Mode, N'NULL');
        RETURN;
    END
END
GO

-- ============================================================================
-- 3. sp_Driver
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Driver
    @Mode NVARCHAR(30),
    @Id INT = NULL,
    @Name NVARCHAR(150) = NULL,
    @LicenseNumber NVARCHAR(50) = NULL,
    @LicenseCategory NVARCHAR(20) = NULL,
    @LicenseExpiry DATE = NULL,
    @ContactNumber NVARCHAR(30) = NULL,
    @SafetyScore INT = NULL,
    @Status NVARCHAR(30) = NULL,
    @Search NVARCHAR(100) = NULL,
    @SortBy NVARCHAR(50) = NULL,
    @SortDir NVARCHAR(10) = NULL,
    @ResultCode INT = 0 OUTPUT,
    @Message NVARCHAR(400) = N'' OUTPUT,
    @NewId INT = 0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @ResultCode = 0;
    SET @Message = N'';
    SET @NewId = 0;

    IF @Mode = N'GET_BY_ID'
    BEGIN
        SELECT Id, Name, LicenseNumber, LicenseCategory, LicenseExpiry, ContactNumber, SafetyScore, Status, RowVer
        FROM dbo.Drivers
        WHERE Id = @Id;

        IF @@ROWCOUNT = 0
        BEGIN
            SET @ResultCode = 1;
            SET @Message = N'Driver not found.';
        END
        RETURN;
    END
    ELSE IF @Mode = N'GET_ALL'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX) = N'SELECT Id, Name, LicenseNumber, LicenseCategory, LicenseExpiry, ContactNumber, SafetyScore, Status, RowVer FROM dbo.Drivers WHERE 1=1';
        DECLARE @ParamDef NVARCHAR(MAX) = N'@Status NVARCHAR(30), @Search NVARCHAR(100)';

        IF @Status IS NOT NULL
            SET @Sql += N' AND Status = @Status';
        IF @Search IS NOT NULL AND LEN(TRIM(@Search)) > 0
        BEGIN
            SET @Search = N'%' + TRIM(@Search) + N'%';
            SET @Sql += N' AND (Name LIKE @Search OR LicenseNumber LIKE @Search)';
        END

        DECLARE @OrderCol NVARCHAR(50) = CASE LOWER(ISNULL(@SortBy, N'id'))
            WHEN N'name' THEN N'Name'
            WHEN N'licensenumber' THEN N'LicenseNumber'
            WHEN N'licenseexpiry' THEN N'LicenseExpiry'
            WHEN N'safetyscore' THEN N'SafetyScore'
            WHEN N'status' THEN N'Status'
            ELSE N'Id'
        END;

        DECLARE @OrderDirection NVARCHAR(4) = CASE UPPER(ISNULL(@SortDir, N'ASC'))
            WHEN N'DESC' THEN N'DESC'
            ELSE N'ASC'
        END;

        SET @Sql += N' ORDER BY ' + @OrderCol + N' ' + @OrderDirection;

        EXEC sp_executesql @Sql, @ParamDef, @Status = @Status, @Search = @Search;
        RETURN;
    END
    ELSE IF @Mode = N'GET_AVAILABLE'
    BEGIN
        -- Dispatch pool: Available AND license expiry >= today (CAST(GETDATE() AS DATE))
        SELECT Id, Name, LicenseNumber, LicenseCategory, LicenseExpiry, ContactNumber, SafetyScore, Status, RowVer
        FROM dbo.Drivers
        WHERE Status = 'Available' AND LicenseExpiry >= CAST(GETDATE() AS DATE)
        ORDER BY Name ASC;
        RETURN;
    END
    ELSE IF @Mode = N'INSERT'
    BEGIN
        BEGIN TRY
            IF EXISTS (SELECT 1 FROM dbo.Drivers WHERE LicenseNumber = @LicenseNumber)
            BEGIN
                SET @ResultCode = 3;
                SET @Message = N'Driver with license number ''' + @LicenseNumber + N''' already exists.';
                RETURN;
            END

            INSERT INTO dbo.Drivers (Name, LicenseNumber, LicenseCategory, LicenseExpiry, ContactNumber, SafetyScore, Status)
            VALUES (@Name, @LicenseNumber, @LicenseCategory, @LicenseExpiry, @ContactNumber, @SafetyScore, ISNULL(@Status, 'Available'));

            SET @NewId = SCOPE_IDENTITY();
            SET @ResultCode = 0;
            SET @Message = N'Driver registered successfully.';
        END TRY
        BEGIN CATCH
            IF ERROR_NUMBER() IN (2601, 2627)
            BEGIN
                SET @ResultCode = 3;
                SET @Message = N'Driver with license number ''' + @LicenseNumber + N''' already exists.';
            END
            ELSE
            BEGIN
                SET @ResultCode = 2;
                SET @Message = ERROR_MESSAGE();
            END
        END CATCH
        RETURN;
    END
    ELSE IF @Mode = N'UPDATE'
    BEGIN
        BEGIN TRY
            IF NOT EXISTS (SELECT 1 FROM dbo.Drivers WHERE Id = @Id)
            BEGIN
                SET @ResultCode = 1;
                SET @Message = N'Driver not found.';
                RETURN;
            END

            IF EXISTS (SELECT 1 FROM dbo.Drivers WHERE LicenseNumber = @LicenseNumber AND Id <> @Id)
            BEGIN
                SET @ResultCode = 3;
                SET @Message = N'Driver with license number ''' + @LicenseNumber + N''' already exists.';
                RETURN;
            END

            UPDATE dbo.Drivers
            SET Name = @Name,
                LicenseNumber = @LicenseNumber,
                LicenseCategory = @LicenseCategory,
                LicenseExpiry = @LicenseExpiry,
                ContactNumber = @ContactNumber,
                SafetyScore = @SafetyScore,
                Status = ISNULL(@Status, Status)
            WHERE Id = @Id;

            SET @ResultCode = 0;
            SET @Message = N'Driver updated successfully.';
        END TRY
        BEGIN CATCH
            IF ERROR_NUMBER() IN (2601, 2627)
            BEGIN
                SET @ResultCode = 3;
                SET @Message = N'Duplicate driver license number.';
            END
            ELSE
            BEGIN
                SET @ResultCode = 2;
                SET @Message = ERROR_MESSAGE();
            END
        END CATCH
        RETURN;
    END
    ELSE IF @Mode = N'UPDATE_STATUS'
    BEGIN
        BEGIN TRY
            IF NOT EXISTS (SELECT 1 FROM dbo.Drivers WHERE Id = @Id)
            BEGIN
                SET @ResultCode = 1;
                SET @Message = N'Driver not found.';
                RETURN;
            END

            UPDATE dbo.Drivers
            SET Status = @Status
            WHERE Id = @Id;

            SET @ResultCode = 0;
            SET @Message = N'Driver status updated.';
        END TRY
        BEGIN CATCH
            SET @ResultCode = 2;
            SET @Message = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END
    ELSE
    BEGIN
        SET @ResultCode = 2;
        SET @Message = N'Invalid mode: ' + ISNULL(@Mode, N'NULL');
        RETURN;
    END
END
GO

-- ============================================================================
-- 4. sp_Trip
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Trip
    @Mode NVARCHAR(30),
    @Id INT = NULL,
    @Source NVARCHAR(150) = NULL,
    @Destination NVARCHAR(150) = NULL,
    @VehicleId INT = NULL,
    @DriverId INT = NULL,
    @CargoWeightKg DECIMAL(18,2) = NULL,
    @PlannedDistanceKm DECIMAL(18,2) = NULL,
    @Revenue DECIMAL(18,2) = NULL,
    @Status NVARCHAR(30) = NULL,
    @FinalOdometer DECIMAL(18,2) = NULL,
    @FuelConsumedL DECIMAL(18,2) = NULL,
    @CreatedBy INT = NULL,
    @ResultCode INT = 0 OUTPUT,
    @Message NVARCHAR(400) = N'' OUTPUT,
    @NewId INT = 0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @ResultCode = 0;
    SET @Message = N'';
    SET @NewId = 0;

    IF @Mode = N'GET_BY_ID'
    BEGIN
        SELECT t.Id, t.Source, t.Destination, t.VehicleId, v.RegistrationNumber AS VehicleRegistrationNumber,
               t.DriverId, d.Name AS DriverName, t.CargoWeightKg, t.PlannedDistanceKm, t.Revenue,
               t.Status, t.FinalOdometer, t.FuelConsumedL, t.ActualDistanceKm, t.CreatedBy, u.FullName AS CreatedByName,
               t.CreatedAt, t.DispatchedAt, t.CompletedAt, t.RowVer
        FROM dbo.Trips t
        INNER JOIN dbo.Vehicles v ON t.VehicleId = v.Id
        INNER JOIN dbo.Drivers d ON t.DriverId = d.Id
        INNER JOIN dbo.Users u ON t.CreatedBy = u.Id
        WHERE t.Id = @Id;

        IF @@ROWCOUNT = 0
        BEGIN
            SET @ResultCode = 1;
            SET @Message = N'Trip not found.';
        END
        RETURN;
    END
    ELSE IF @Mode = N'GET_ALL'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX) = N'
            SELECT t.Id, t.Source, t.Destination, t.VehicleId, v.RegistrationNumber AS VehicleRegistrationNumber,
                   t.DriverId, d.Name AS DriverName, t.CargoWeightKg, t.PlannedDistanceKm, t.Revenue,
                   t.Status, t.FinalOdometer, t.FuelConsumedL, t.ActualDistanceKm, t.CreatedBy, u.FullName AS CreatedByName,
                   t.CreatedAt, t.DispatchedAt, t.CompletedAt, t.RowVer
            FROM dbo.Trips t
            INNER JOIN dbo.Vehicles v ON t.VehicleId = v.Id
            INNER JOIN dbo.Drivers d ON t.DriverId = d.Id
            INNER JOIN dbo.Users u ON t.CreatedBy = u.Id
            WHERE 1=1';

        DECLARE @ParamDef NVARCHAR(MAX) = N'@Status NVARCHAR(30), @VehicleId INT, @DriverId INT';

        IF @Status IS NOT NULL
            SET @Sql += N' AND t.Status = @Status';
        IF @VehicleId IS NOT NULL
            SET @Sql += N' AND t.VehicleId = @VehicleId';
        IF @DriverId IS NOT NULL
            SET @Sql += N' AND t.DriverId = @DriverId';

        SET @Sql += N' ORDER BY t.Id DESC';

        EXEC sp_executesql @Sql, @ParamDef, @Status = @Status, @VehicleId = @VehicleId, @DriverId = @DriverId;
        RETURN;
    END
    ELSE IF @Mode = N'INSERT'
    BEGIN
        BEGIN TRY
            -- Validate vehicle existence & status & load capacity
            DECLARE @vStatus NVARCHAR(30), @vMaxLoad DECIMAL(18,2);
            SELECT @vStatus = Status, @vMaxLoad = MaxLoadKg
            FROM dbo.Vehicles WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @VehicleId;

            IF @vStatus IS NULL
            BEGIN
                SET @ResultCode = 1;
                SET @Message = N'Vehicle not found.';
                RETURN;
            END

            IF @vStatus <> 'Available'
            BEGIN
                SET @ResultCode = 2;
                SET @Message = N'Vehicle is not Available for assignment (Status: ' + @vStatus + N').';
                RETURN;
            END

            IF @CargoWeightKg > @vMaxLoad
            BEGIN
                SET @ResultCode = 2;
                SET @Message = N'Cargo weight (' + CAST(@CargoWeightKg AS NVARCHAR(20)) + N' kg) exceeds vehicle max load (' + CAST(@vMaxLoad AS NVARCHAR(20)) + N' kg).';
                RETURN;
            END

            -- Validate driver existence & status & license expiry
            DECLARE @dStatus NVARCHAR(30), @dExpiry DATE;
            SELECT @dStatus = Status, @dExpiry = LicenseExpiry
            FROM dbo.Drivers WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @DriverId;

            IF @dStatus IS NULL
            BEGIN
                SET @ResultCode = 1;
                SET @Message = N'Driver not found.';
                RETURN;
            END

            IF @dStatus <> 'Available'
            BEGIN
                SET @ResultCode = 2;
                SET @Message = N'Driver is not Available for assignment (Status: ' + @dStatus + N').';
                RETURN;
            END

            IF @dExpiry < CAST(GETDATE() AS DATE)
            BEGIN
                SET @ResultCode = 2;
                SET @Message = N'Driver license is expired (Expiry: ' + CONVERT(NVARCHAR(30), @dExpiry, 23) + N').';
                RETURN;
            END

            INSERT INTO dbo.Trips (Source, Destination, VehicleId, DriverId, CargoWeightKg, PlannedDistanceKm, Revenue, Status, CreatedBy, CreatedAt)
            VALUES (@Source, @Destination, @VehicleId, @DriverId, @CargoWeightKg, @PlannedDistanceKm, ISNULL(@Revenue, 0), 'Draft', @CreatedBy, SYSUTCDATETIME());

            SET @NewId = SCOPE_IDENTITY();
            SET @ResultCode = 0;
            SET @Message = N'Trip created successfully in Draft status.';
        END TRY
        BEGIN CATCH
            SET @ResultCode = 2;
            SET @Message = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END
    ELSE IF @Mode = N'UPDATE'
    BEGIN
        BEGIN TRY
            DECLARE @currStatus NVARCHAR(30);
            SELECT @currStatus = Status FROM dbo.Trips WHERE Id = @Id;

            IF @currStatus IS NULL
            BEGIN
                SET @ResultCode = 1;
                SET @Message = N'Trip not found.';
                RETURN;
            END

            IF @currStatus <> 'Draft'
            BEGIN
                SET @ResultCode = 2;
                SET @Message = N'Only trips in Draft status can be updated.';
                RETURN;
            END

            -- Check load if cargo changed
            DECLARE @vMaxLoadKg DECIMAL(18,2);
            SELECT @vMaxLoadKg = MaxLoadKg FROM dbo.Vehicles WHERE Id = @VehicleId;
            IF @CargoWeightKg > @vMaxLoadKg
            BEGIN
                SET @ResultCode = 2;
                SET @Message = N'Cargo weight exceeds vehicle capacity.';
                RETURN;
            END

            UPDATE dbo.Trips
            SET Source = @Source,
                Destination = @Destination,
                VehicleId = @VehicleId,
                DriverId = @DriverId,
                CargoWeightKg = @CargoWeightKg,
                PlannedDistanceKm = @PlannedDistanceKm,
                Revenue = ISNULL(@Revenue, Revenue)
            WHERE Id = @Id;

            SET @ResultCode = 0;
            SET @Message = N'Trip updated successfully.';
        END TRY
        BEGIN CATCH
            SET @ResultCode = 2;
            SET @Message = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END
    ELSE IF @Mode = N'DISPATCH'
    BEGIN
        BEGIN TRANSACTION;
        BEGIN TRY
            DECLARE @tTripStatus NVARCHAR(30), @tVehId INT, @tDrvId INT, @tCargo DECIMAL(18,2);
            SELECT @tTripStatus = Status, @tVehId = VehicleId, @tDrvId = DriverId, @tCargo = CargoWeightKg
            FROM dbo.Trips WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @Id;

            IF @tTripStatus IS NULL
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 1;
                SET @Message = N'Trip not found.';
                RETURN;
            END

            IF @tTripStatus <> 'Draft'
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Cannot dispatch trip in ''' + @tTripStatus + N''' status. Trip must be Draft.';
                RETURN;
            END

            -- Check vehicle
            DECLARE @vCurrentStatus NVARCHAR(30), @vCapacity DECIMAL(18,2);
            SELECT @vCurrentStatus = Status, @vCapacity = MaxLoadKg
            FROM dbo.Vehicles WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @tVehId;

            IF @vCurrentStatus <> 'Available'
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Vehicle is not Available (current status: ' + @vCurrentStatus + N'). Cannot dispatch.';
                RETURN;
            END

            IF @tCargo > @vCapacity
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Cargo weight exceeds vehicle max load.';
                RETURN;
            END

            -- Check driver
            DECLARE @dCurrentStatus NVARCHAR(30), @dLicenseExpiry DATE;
            SELECT @dCurrentStatus = Status, @dLicenseExpiry = LicenseExpiry
            FROM dbo.Drivers WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @tDrvId;

            IF @dCurrentStatus <> 'Available'
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Driver is not Available (current status: ' + @dCurrentStatus + N'). Cannot dispatch.';
                RETURN;
            END

            IF @dLicenseExpiry < CAST(GETDATE() AS DATE)
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Driver license is expired. Cannot dispatch.';
                RETURN;
            END

            -- Update atomically
            UPDATE dbo.Trips
            SET Status = 'Dispatched',
                DispatchedAt = SYSUTCDATETIME()
            WHERE Id = @Id;

            UPDATE dbo.Vehicles
            SET Status = 'OnTrip'
            WHERE Id = @tVehId;

            UPDATE dbo.Drivers
            SET Status = 'OnTrip'
            WHERE Id = @tDrvId;

            COMMIT TRANSACTION;
            SET @ResultCode = 0;
            SET @Message = N'Trip dispatched successfully.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            IF ERROR_NUMBER() IN (2601, 2627)
            BEGIN
                SET @ResultCode = 3;
                SET @Message = N'Conflict: Vehicle or driver is already on another active dispatched trip.';
            END
            ELSE
            BEGIN
                SET @ResultCode = 2;
                SET @Message = ERROR_MESSAGE();
            END
        END CATCH
        RETURN;
    END
    ELSE IF @Mode = N'COMPLETE'
    BEGIN
        BEGIN TRANSACTION;
        BEGIN TRY
            DECLARE @cTripStatus NVARCHAR(30), @cVehId INT, @cDrvId INT;
            SELECT @cTripStatus = Status, @cVehId = VehicleId, @cDrvId = DriverId
            FROM dbo.Trips WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @Id;

            IF @cTripStatus IS NULL
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 1;
                SET @Message = N'Trip not found.';
                RETURN;
            END

            IF @cTripStatus <> 'Dispatched'
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Cannot complete trip in ''' + @cTripStatus + N''' status. Trip must be Dispatched.';
                RETURN;
            END

            IF @FuelConsumedL IS NULL OR @FuelConsumedL <= 0
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Fuel consumed must be greater than 0.';
                RETURN;
            END

            DECLARE @vOldOdometer DECIMAL(18,2);
            SELECT @vOldOdometer = Odometer
            FROM dbo.Vehicles WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @cVehId;

            IF @FinalOdometer IS NULL OR @FinalOdometer < @vOldOdometer
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Final odometer (' + CAST(ISNULL(@FinalOdometer,0) AS NVARCHAR(20)) + N') cannot be less than current vehicle odometer (' + CAST(@vOldOdometer AS NVARCHAR(20)) + N').';
                RETURN;
            END

            DECLARE @actualDistance DECIMAL(18,2) = @FinalOdometer - @vOldOdometer;

            -- Update Trip
            UPDATE dbo.Trips
            SET Status = 'Completed',
                CompletedAt = SYSUTCDATETIME(),
                FinalOdometer = @FinalOdometer,
                FuelConsumedL = @FuelConsumedL,
                ActualDistanceKm = @actualDistance
            WHERE Id = @Id;

            -- Update Vehicle: Odometer and set back to Available
            UPDATE dbo.Vehicles
            SET Odometer = @FinalOdometer,
                Status = 'Available'
            WHERE Id = @cVehId;

            -- Update Driver: back to Available
            UPDATE dbo.Drivers
            SET Status = 'Available'
            WHERE Id = @cDrvId;

            COMMIT TRANSACTION;
            SET @ResultCode = 0;
            SET @Message = N'Trip completed successfully.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            SET @ResultCode = 2;
            SET @Message = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END
    ELSE IF @Mode = N'CANCEL'
    BEGIN
        BEGIN TRANSACTION;
        BEGIN TRY
            DECLARE @cnTripStatus NVARCHAR(30), @cnVehId INT, @cnDrvId INT;
            SELECT @cnTripStatus = Status, @cnVehId = VehicleId, @cnDrvId = DriverId
            FROM dbo.Trips WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @Id;

            IF @cnTripStatus IS NULL
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 1;
                SET @Message = N'Trip not found.';
                RETURN;
            END

            IF @cnTripStatus IN ('Completed', 'Cancelled')
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Cannot cancel trip in ''' + @cnTripStatus + N''' status.';
                RETURN;
            END

            IF @cnTripStatus = 'Draft'
            BEGIN
                UPDATE dbo.Trips SET Status = 'Cancelled' WHERE Id = @Id;
            END
            ELSE IF @cnTripStatus = 'Dispatched'
            BEGIN
                UPDATE dbo.Trips SET Status = 'Cancelled' WHERE Id = @Id;

                -- Restore Vehicle to Available
                UPDATE dbo.Vehicles WITH (UPDLOCK, ROWLOCK)
                SET Status = 'Available'
                WHERE Id = @cnVehId;

                -- Restore Driver to Available
                UPDATE dbo.Drivers WITH (UPDLOCK, ROWLOCK)
                SET Status = 'Available'
                WHERE Id = @cnDrvId;
            END

            COMMIT TRANSACTION;
            SET @ResultCode = 0;
            SET @Message = N'Trip cancelled successfully.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            SET @ResultCode = 2;
            SET @Message = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END
    ELSE
    BEGIN
        SET @ResultCode = 2;
        SET @Message = N'Invalid mode: ' + ISNULL(@Mode, N'NULL');
        RETURN;
    END
END
GO

-- ============================================================================
-- 5. sp_Maintenance
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Maintenance
    @Mode NVARCHAR(30),
    @Id INT = NULL,
    @VehicleId INT = NULL,
    @Description NVARCHAR(255) = NULL,
    @Cost DECIMAL(18,2) = NULL,
    @ResultCode INT = 0 OUTPUT,
    @Message NVARCHAR(400) = N'' OUTPUT,
    @NewId INT = 0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @ResultCode = 0;
    SET @Message = N'';
    SET @NewId = 0;

    IF @Mode = N'GET_BY_ID'
    BEGIN
        SELECT m.Id, m.VehicleId, v.RegistrationNumber AS VehicleRegistrationNumber,
               m.Description, m.Cost, m.StartDate, m.EndDate, m.Status
        FROM dbo.MaintenanceLogs m
        INNER JOIN dbo.Vehicles v ON m.VehicleId = v.Id
        WHERE m.Id = @Id;

        IF @@ROWCOUNT = 0
        BEGIN
            SET @ResultCode = 1;
            SET @Message = N'Maintenance record not found.';
        END
        RETURN;
    END
    ELSE IF @Mode = N'GET_ALL'
    BEGIN
        SELECT m.Id, m.VehicleId, v.RegistrationNumber AS VehicleRegistrationNumber,
               m.Description, m.Cost, m.StartDate, m.EndDate, m.Status
        FROM dbo.MaintenanceLogs m
        INNER JOIN dbo.Vehicles v ON m.VehicleId = v.Id
        ORDER BY m.Id DESC;
        RETURN;
    END
    ELSE IF @Mode = N'INSERT'
    BEGIN
        BEGIN TRANSACTION;
        BEGIN TRY
            DECLARE @vehStatus NVARCHAR(30);
            SELECT @vehStatus = Status
            FROM dbo.Vehicles WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @VehicleId;

            IF @vehStatus IS NULL
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 1;
                SET @Message = N'Vehicle not found.';
                RETURN;
            END

            IF @vehStatus = 'OnTrip'
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Cannot send vehicle for maintenance while it is On Trip.';
                RETURN;
            END

            IF @vehStatus = 'Retired'
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Cannot create maintenance for Retired vehicle.';
                RETURN;
            END

            IF @vehStatus = 'InShop'
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 3;
                SET @Message = N'Vehicle is already In Shop under active maintenance.';
                RETURN;
            END

            -- Check active maintenance record backstop
            IF EXISTS (SELECT 1 FROM dbo.MaintenanceLogs WHERE VehicleId = @VehicleId AND Status = 'Active')
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 3;
                SET @Message = N'Vehicle already has an Active maintenance record.';
                RETURN;
            END

            INSERT INTO dbo.MaintenanceLogs (VehicleId, Description, Cost, StartDate, Status)
            VALUES (@VehicleId, @Description, @Cost, SYSUTCDATETIME(), 'Active');

            SET @NewId = SCOPE_IDENTITY();

            -- Set vehicle to InShop
            UPDATE dbo.Vehicles
            SET Status = 'InShop'
            WHERE Id = @VehicleId;

            COMMIT TRANSACTION;
            SET @ResultCode = 0;
            SET @Message = N'Maintenance record created and vehicle moved to InShop.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            IF ERROR_NUMBER() IN (2601, 2627)
            BEGIN
                SET @ResultCode = 3;
                SET @Message = N'Conflict: Active maintenance record already exists for this vehicle.';
            END
            ELSE
            BEGIN
                SET @ResultCode = 2;
                SET @Message = ERROR_MESSAGE();
            END
        END CATCH
        RETURN;
    END
    ELSE IF @Mode = N'CLOSE'
    BEGIN
        BEGIN TRANSACTION;
        BEGIN TRY
            DECLARE @mStatus NVARCHAR(30), @mVehId INT;
            SELECT @mStatus = Status, @mVehId = VehicleId
            FROM dbo.MaintenanceLogs WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @Id;

            IF @mStatus IS NULL
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 1;
                SET @Message = N'Maintenance record not found.';
                RETURN;
            END

            IF @mStatus = 'Closed'
            BEGIN
                ROLLBACK TRANSACTION;
                SET @ResultCode = 2;
                SET @Message = N'Maintenance record is already closed.';
                RETURN;
            END

            UPDATE dbo.MaintenanceLogs
            SET Status = 'Closed',
                EndDate = SYSUTCDATETIME()
            WHERE Id = @Id;

            -- Check vehicle status: restore to Available unless Retired
            DECLARE @currentVehStatus NVARCHAR(30);
            SELECT @currentVehStatus = Status
            FROM dbo.Vehicles WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @mVehId;

            IF @currentVehStatus <> 'Retired'
            BEGIN
                UPDATE dbo.Vehicles
                SET Status = 'Available'
                WHERE Id = @mVehId;
            END

            COMMIT TRANSACTION;
            SET @ResultCode = 0;
            SET @Message = N'Maintenance record closed successfully.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            SET @ResultCode = 2;
            SET @Message = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END
    ELSE
    BEGIN
        SET @ResultCode = 2;
        SET @Message = N'Invalid mode: ' + ISNULL(@Mode, N'NULL');
        RETURN;
    END
END
GO

-- ============================================================================
-- 6. sp_FuelLog
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_FuelLog
    @Mode NVARCHAR(30),
    @VehicleId INT = NULL,
    @TripId INT = NULL,
    @Liters DECIMAL(18,2) = NULL,
    @Cost DECIMAL(18,2) = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @ResultCode INT = 0 OUTPUT,
    @Message NVARCHAR(400) = N'' OUTPUT,
    @NewId INT = 0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @ResultCode = 0;
    SET @Message = N'';
    SET @NewId = 0;

    IF @Mode = N'GET_ALL'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX) = N'
            SELECT f.Id, f.VehicleId, v.RegistrationNumber AS VehicleRegistrationNumber,
                   f.TripId, f.Liters, f.Cost, f.LogDate
            FROM dbo.FuelLogs f
            INNER JOIN dbo.Vehicles v ON f.VehicleId = v.Id
            WHERE 1=1';

        DECLARE @ParamDef NVARCHAR(MAX) = N'@VehicleId INT, @StartDate DATETIME2, @EndDate DATETIME2';

        IF @VehicleId IS NOT NULL
            SET @Sql += N' AND f.VehicleId = @VehicleId';
        IF @StartDate IS NOT NULL
            SET @Sql += N' AND f.LogDate >= @StartDate';
        IF @EndDate IS NOT NULL
            SET @Sql += N' AND f.LogDate <= @EndDate';

        SET @Sql += N' ORDER BY f.LogDate DESC';

        EXEC sp_executesql @Sql, @ParamDef, @VehicleId = @VehicleId, @StartDate = @StartDate, @EndDate = @EndDate;
        RETURN;
    END
    ELSE IF @Mode = N'INSERT'
    BEGIN
        BEGIN TRY
            IF NOT EXISTS (SELECT 1 FROM dbo.Vehicles WHERE Id = @VehicleId)
            BEGIN
                SET @ResultCode = 1;
                SET @Message = N'Vehicle not found.';
                RETURN;
            END

            IF @TripId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Trips WHERE Id = @TripId)
            BEGIN
                SET @ResultCode = 1;
                SET @Message = N'Trip not found.';
                RETURN;
            END

            INSERT INTO dbo.FuelLogs (VehicleId, TripId, Liters, Cost, LogDate)
            VALUES (@VehicleId, @TripId, @Liters, @Cost, SYSUTCDATETIME());

            SET @NewId = SCOPE_IDENTITY();
            SET @ResultCode = 0;
            SET @Message = N'Fuel log added successfully.';
        END TRY
        BEGIN CATCH
            SET @ResultCode = 2;
            SET @Message = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END
    ELSE
    BEGIN
        SET @ResultCode = 2;
        SET @Message = N'Invalid mode: ' + ISNULL(@Mode, N'NULL');
        RETURN;
    END
END
GO

-- ============================================================================
-- 7. sp_Expense
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Expense
    @Mode NVARCHAR(30),
    @VehicleId INT = NULL,
    @TripId INT = NULL,
    @Type NVARCHAR(30) = NULL,
    @Amount DECIMAL(18,2) = NULL,
    @Notes NVARCHAR(500) = NULL,
    @ResultCode INT = 0 OUTPUT,
    @Message NVARCHAR(400) = N'' OUTPUT,
    @NewId INT = 0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @ResultCode = 0;
    SET @Message = N'';
    SET @NewId = 0;

    IF @Mode = N'GET_ALL'
    BEGIN
        SELECT e.Id, e.VehicleId, v.RegistrationNumber AS VehicleRegistrationNumber,
               e.TripId, e.Type, e.Amount, e.ExpenseDate, e.Notes
        FROM dbo.Expenses e
        INNER JOIN dbo.Vehicles v ON e.VehicleId = v.Id
        ORDER BY e.ExpenseDate DESC;
        RETURN;
    END
    ELSE IF @Mode = N'INSERT'
    BEGIN
        BEGIN TRY
            IF NOT EXISTS (SELECT 1 FROM dbo.Vehicles WHERE Id = @VehicleId)
            BEGIN
                SET @ResultCode = 1;
                SET @Message = N'Vehicle not found.';
                RETURN;
            END

            IF @TripId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Trips WHERE Id = @TripId)
            BEGIN
                SET @ResultCode = 1;
                SET @Message = N'Trip not found.';
                RETURN;
            END

            INSERT INTO dbo.Expenses (VehicleId, TripId, Type, Amount, ExpenseDate, Notes)
            VALUES (@VehicleId, @TripId, @Type, @Amount, SYSUTCDATETIME(), @Notes);

            SET @NewId = SCOPE_IDENTITY();
            SET @ResultCode = 0;
            SET @Message = N'Expense recorded successfully.';
        END TRY
        BEGIN CATCH
            SET @ResultCode = 2;
            SET @Message = ERROR_MESSAGE();
        END CATCH
        RETURN;
    END
    ELSE
    BEGIN
        SET @ResultCode = 2;
        SET @Message = N'Invalid mode: ' + ISNULL(@Mode, N'NULL');
        RETURN;
    END
END
GO

-- ============================================================================
-- 8. sp_Dashboard
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Dashboard
    @Mode NVARCHAR(30),
    @VehicleType NVARCHAR(50) = NULL,
    @Status NVARCHAR(30) = NULL,
    @Region NVARCHAR(50) = NULL,
    @ResultCode INT = 0 OUTPUT,
    @Message NVARCHAR(400) = N'' OUTPUT,
    @NewId INT = 0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @ResultCode = 0;
    SET @Message = N'';
    SET @NewId = 0;

    IF @Mode = N'KPI'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX) = N'
            WITH FilteredVehicles AS (
                SELECT Id, Status
                FROM dbo.Vehicles
                WHERE 1=1' +
                CASE WHEN @VehicleType IS NOT NULL THEN N' AND Type = @VehicleType' ELSE N'' END +
                CASE WHEN @Status IS NOT NULL THEN N' AND Status = @Status' ELSE N'' END +
                CASE WHEN @Region IS NOT NULL THEN N' AND Region = @Region' ELSE N'' END +
            N')
            SELECT 
                (SELECT COUNT(1) FROM FilteredVehicles WHERE Status = ''OnTrip'') AS ActiveVehicles,
                (SELECT COUNT(1) FROM FilteredVehicles WHERE Status = ''Available'') AS AvailableVehicles,
                (SELECT COUNT(1) FROM FilteredVehicles WHERE Status = ''InShop'') AS VehiclesInMaintenance,
                (SELECT COUNT(1) FROM dbo.Trips WHERE Status = ''Dispatched'') AS ActiveTrips,
                (SELECT COUNT(1) FROM dbo.Trips WHERE Status = ''Draft'') AS PendingTrips,
                (SELECT COUNT(1) FROM dbo.Drivers WHERE Status = ''OnTrip'') AS DriversOnDuty,
                (SELECT CASE 
                    WHEN COUNT(1) = 0 THEN 0.0
                    ELSE ROUND(CAST(SUM(CASE WHEN Status = ''OnTrip'' THEN 1 ELSE 0 END) AS FLOAT) / CAST(COUNT(1) AS FLOAT) * 100.0, 2)
                 END
                 FROM FilteredVehicles
                 WHERE Status <> ''Retired''
                ) AS FleetUtilizationPercent;';

        DECLARE @ParamDef NVARCHAR(MAX) = N'@VehicleType NVARCHAR(50), @Status NVARCHAR(30), @Region NVARCHAR(50)';
        EXEC sp_executesql @Sql, @ParamDef, @VehicleType = @VehicleType, @Status = @Status, @Region = @Region;
        RETURN;
    END
    ELSE
    BEGIN
        SET @ResultCode = 2;
        SET @Message = N'Invalid mode: ' + ISNULL(@Mode, N'NULL');
        RETURN;
    END
END
GO

-- ============================================================================
-- 9. sp_Report
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_Report
    @Mode NVARCHAR(30),
    @VehicleId INT = NULL,
    @ResultCode INT = 0 OUTPUT,
    @Message NVARCHAR(400) = N'' OUTPUT,
    @NewId INT = 0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @ResultCode = 0;
    SET @Message = N'';
    SET @NewId = 0;

    IF @Mode = N'FUEL_EFFICIENCY'
    BEGIN
        -- Fuel Efficiency = total distance / total fuel (completed trips)
        SELECT 
            v.Id AS VehicleId,
            v.RegistrationNumber,
            v.Name AS VehicleName,
            v.Type AS VehicleType,
            ISNULL(SUM(t.ActualDistanceKm), 0) AS TotalDistanceKm,
            ISNULL(SUM(t.FuelConsumedL), 0) AS TotalFuelConsumedL,
            CASE 
                WHEN ISNULL(SUM(t.FuelConsumedL), 0) = 0 THEN 0.0
                ELSE ROUND(CAST(ISNULL(SUM(t.ActualDistanceKm), 0) AS FLOAT) / CAST(SUM(t.FuelConsumedL) AS FLOAT), 2)
            END AS KmPerLiter
        FROM dbo.Vehicles v
        LEFT JOIN dbo.Trips t ON v.Id = t.VehicleId AND t.Status = 'Completed'
        GROUP BY v.Id, v.RegistrationNumber, v.Name, v.Type
        ORDER BY v.RegistrationNumber ASC;
        RETURN;
    END
    ELSE IF @Mode = N'UTILIZATION'
    BEGIN
        -- Utilization per vehicle: completed trips count, total distance, total revenue
        SELECT 
            v.Id AS VehicleId,
            v.RegistrationNumber,
            v.Name AS VehicleName,
            v.Status AS CurrentStatus,
            COUNT(t.Id) AS CompletedTripsCount,
            ISNULL(SUM(t.ActualDistanceKm), 0) AS TotalDistanceKm,
            ISNULL(SUM(t.Revenue), 0) AS TotalRevenue
        FROM dbo.Vehicles v
        LEFT JOIN dbo.Trips t ON v.Id = t.VehicleId AND t.Status = 'Completed'
        GROUP BY v.Id, v.RegistrationNumber, v.Name, v.Status
        ORDER BY v.RegistrationNumber ASC;
        RETURN;
    END
    ELSE IF @Mode = N'COST'
    BEGIN
        -- Operational Cost = SUM(FuelLogs.Cost) + SUM(MaintenanceLogs.Cost).
        -- Separate OtherExpenses = SUM(Expenses where Type != 'Maintenance') to avoid double counting.
        SELECT 
            v.Id AS VehicleId,
            v.RegistrationNumber,
            v.Name AS VehicleName,
            ISNULL((SELECT SUM(f.Cost) FROM dbo.FuelLogs f WHERE f.VehicleId = v.Id), 0) AS TotalFuelCost,
            ISNULL((SELECT SUM(m.Cost) FROM dbo.MaintenanceLogs m WHERE m.VehicleId = v.Id), 0) AS TotalMaintenanceCost,
            (ISNULL((SELECT SUM(f.Cost) FROM dbo.FuelLogs f WHERE f.VehicleId = v.Id), 0) + 
             ISNULL((SELECT SUM(m.Cost) FROM dbo.MaintenanceLogs m WHERE m.VehicleId = v.Id), 0)) AS TotalOperationalCost,
            ISNULL((SELECT SUM(e.Amount) FROM dbo.Expenses e WHERE e.VehicleId = v.Id AND e.Type <> 'Maintenance'), 0) AS OtherExpensesCost
        FROM dbo.Vehicles v
        ORDER BY v.RegistrationNumber ASC;
        RETURN;
    END
    ELSE IF @Mode = N'ROI'
    BEGIN
        -- Vehicle ROI = (Revenue - (Maintenance + Fuel)) / AcquisitionCost
        SELECT 
            v.Id AS VehicleId,
            v.RegistrationNumber,
            v.Name AS VehicleName,
            v.AcquisitionCost,
            ISNULL((SELECT SUM(t.Revenue) FROM dbo.Trips t WHERE t.VehicleId = v.Id AND t.Status = 'Completed'), 0) AS TotalRevenue,
            ISNULL((SELECT SUM(f.Cost) FROM dbo.FuelLogs f WHERE f.VehicleId = v.Id), 0) AS TotalFuelCost,
            ISNULL((SELECT SUM(m.Cost) FROM dbo.MaintenanceLogs m WHERE m.VehicleId = v.Id), 0) AS TotalMaintenanceCost,
            (ISNULL((SELECT SUM(f.Cost) FROM dbo.FuelLogs f WHERE f.VehicleId = v.Id), 0) + 
             ISNULL((SELECT SUM(m.Cost) FROM dbo.MaintenanceLogs m WHERE m.VehicleId = v.Id), 0)) AS TotalOperationalCost,
            CASE 
                WHEN v.AcquisitionCost = 0 THEN 0.0
                ELSE ROUND(
                    (ISNULL((SELECT SUM(t.Revenue) FROM dbo.Trips t WHERE t.VehicleId = v.Id AND t.Status = 'Completed'), 0)
                     - (ISNULL((SELECT SUM(f.Cost) FROM dbo.FuelLogs f WHERE f.VehicleId = v.Id), 0) + 
                        ISNULL((SELECT SUM(m.Cost) FROM dbo.MaintenanceLogs m WHERE m.VehicleId = v.Id), 0))
                    ) / CAST(v.AcquisitionCost AS FLOAT), 4)
            END AS RoiRatio
        FROM dbo.Vehicles v
        ORDER BY v.RegistrationNumber ASC;
        RETURN;
    END
    ELSE IF @Mode = N'VEHICLE_ANALYTICS'
    BEGIN
        IF @VehicleId IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.Vehicles WHERE Id = @VehicleId)
        BEGIN
            SET @ResultCode = 1;
            SET @Message = N'Vehicle not found.';
            RETURN;
        END

        SELECT 
            v.Id AS VehicleId,
            v.RegistrationNumber,
            v.Name AS VehicleName,
            v.Type AS VehicleType,
            v.Status AS CurrentStatus,
            v.Odometer,
            v.AcquisitionCost,
            ISNULL((SELECT SUM(t.Revenue) FROM dbo.Trips t WHERE t.VehicleId = v.Id AND t.Status = 'Completed'), 0) AS TotalRevenue,
            ISNULL((SELECT SUM(t.ActualDistanceKm) FROM dbo.Trips t WHERE t.VehicleId = v.Id AND t.Status = 'Completed'), 0) AS TotalDistanceKm,
            ISNULL((SELECT SUM(f.Cost) FROM dbo.FuelLogs f WHERE f.VehicleId = v.Id), 0) AS TotalFuelCost,
            ISNULL((SELECT SUM(m.Cost) FROM dbo.MaintenanceLogs m WHERE m.VehicleId = v.Id), 0) AS TotalMaintenanceCost,
            ISNULL((SELECT SUM(e.Amount) FROM dbo.Expenses e WHERE e.VehicleId = v.Id AND e.Type <> 'Maintenance'), 0) AS OtherExpensesCost,
            (ISNULL((SELECT SUM(f.Cost) FROM dbo.FuelLogs f WHERE f.VehicleId = v.Id), 0) + 
             ISNULL((SELECT SUM(m.Cost) FROM dbo.MaintenanceLogs m WHERE m.VehicleId = v.Id), 0)) AS TotalOperationalCost,
            CASE 
                WHEN ISNULL((SELECT SUM(t.FuelConsumedL) FROM dbo.Trips t WHERE t.VehicleId = v.Id AND t.Status = 'Completed'), 0) = 0 THEN 0.0
                ELSE ROUND(CAST(ISNULL((SELECT SUM(t.ActualDistanceKm) FROM dbo.Trips t WHERE t.VehicleId = v.Id AND t.Status = 'Completed'), 0) AS FLOAT) / 
                           CAST((SELECT SUM(t.FuelConsumedL) FROM dbo.Trips t WHERE t.VehicleId = v.Id AND t.Status = 'Completed') AS FLOAT), 2)
            END AS FuelEfficiencyKmPerL,
            CASE 
                WHEN v.AcquisitionCost = 0 THEN 0.0
                ELSE ROUND(
                    (ISNULL((SELECT SUM(t.Revenue) FROM dbo.Trips t WHERE t.VehicleId = v.Id AND t.Status = 'Completed'), 0)
                     - (ISNULL((SELECT SUM(f.Cost) FROM dbo.FuelLogs f WHERE f.VehicleId = v.Id), 0) + 
                        ISNULL((SELECT SUM(m.Cost) FROM dbo.MaintenanceLogs m WHERE m.VehicleId = v.Id), 0))
                    ) / CAST(v.AcquisitionCost AS FLOAT), 4)
            END AS RoiRatio
        FROM dbo.Vehicles v
        WHERE v.Id = @VehicleId;
        RETURN;
    END
    ELSE
    BEGIN
        SET @ResultCode = 2;
        SET @Message = N'Invalid mode: ' + ISNULL(@Mode, N'NULL');
        RETURN;
    END
END
GO

