using System.Data;
using Microsoft.Data.SqlClient;
using TransitOps.Api.Base;
using TransitOps.Api.Models;
using TransitOps.Api.Repositories.Interfaces;

namespace TransitOps.Api.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "LOGIN_LOOKUP"),
            new SqlParameter("@Email", email)
        };

        return await QuerySingleAsync("dbo.sp_Auth", reader => new User
        {
            Id = GetValue<int>(reader, "Id"),
            FullName = GetValue<string>(reader, "FullName"),
            Email = GetValue<string>(reader, "Email"),
            PasswordHash = GetValue<string>(reader, "PasswordHash"),
            RoleId = GetValue<int>(reader, "RoleId"),
            RoleName = GetValue<string>(reader, "RoleName"),
            IsActive = GetValue<bool>(reader, "IsActive"),
            CreatedAt = GetValue<DateTime>(reader, "CreatedAt")
        }, parameters);
    }

    public async Task<SpResult> RegisterAsync(string fullName, string email, string passwordHash, int roleId)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "REGISTER"),
            new SqlParameter("@FullName", fullName),
            new SqlParameter("@Email", email),
            new SqlParameter("@PasswordHash", passwordHash),
            new SqlParameter("@RoleId", roleId)
        };

        return await ExecuteAsync("dbo.sp_Auth", parameters);
    }
}

public class VehicleRepository : BaseRepository, IVehicleRepository
{
    public VehicleRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_BY_ID"),
            new SqlParameter("@Id", id)
        };

        return await QuerySingleAsync("dbo.sp_Vehicle", MapVehicle, parameters);
    }

    public async Task<List<Vehicle>> GetAllAsync(string? type, string? status, string? region, string? search, string? sortBy, string? sortDir)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_ALL"),
            new SqlParameter("@Type", (object?)type ?? DBNull.Value),
            new SqlParameter("@Status", (object?)status ?? DBNull.Value),
            new SqlParameter("@Region", (object?)region ?? DBNull.Value),
            new SqlParameter("@Search", (object?)search ?? DBNull.Value),
            new SqlParameter("@SortBy", (object?)sortBy ?? DBNull.Value),
            new SqlParameter("@SortDir", (object?)sortDir ?? DBNull.Value)
        };

        return await QueryAsync("dbo.sp_Vehicle", MapVehicle, parameters);
    }

    public async Task<List<Vehicle>> GetAvailableAsync()
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_AVAILABLE")
        };

        return await QueryAsync("dbo.sp_Vehicle", MapVehicle, parameters);
    }

    public async Task<SpResult> CreateAsync(Vehicle vehicle)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "INSERT"),
            new SqlParameter("@RegistrationNumber", vehicle.RegistrationNumber),
            new SqlParameter("@Name", vehicle.Name),
            new SqlParameter("@Type", vehicle.Type),
            new SqlParameter("@Region", vehicle.Region),
            new SqlParameter("@MaxLoadKg", vehicle.MaxLoadKg),
            new SqlParameter("@Odometer", vehicle.Odometer),
            new SqlParameter("@AcquisitionCost", vehicle.AcquisitionCost),
            new SqlParameter("@Status", string.IsNullOrEmpty(vehicle.Status) ? "Available" : vehicle.Status)
        };

        return await ExecuteAsync("dbo.sp_Vehicle", parameters);
    }

    public async Task<SpResult> UpdateAsync(Vehicle vehicle)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "UPDATE"),
            new SqlParameter("@Id", vehicle.Id),
            new SqlParameter("@RegistrationNumber", vehicle.RegistrationNumber),
            new SqlParameter("@Name", vehicle.Name),
            new SqlParameter("@Type", vehicle.Type),
            new SqlParameter("@Region", vehicle.Region),
            new SqlParameter("@MaxLoadKg", vehicle.MaxLoadKg),
            new SqlParameter("@Odometer", vehicle.Odometer),
            new SqlParameter("@AcquisitionCost", vehicle.AcquisitionCost),
            new SqlParameter("@Status", (object?)vehicle.Status ?? DBNull.Value)
        };

        return await ExecuteAsync("dbo.sp_Vehicle", parameters);
    }

    public async Task<SpResult> DeleteAsync(int id)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "DELETE"),
            new SqlParameter("@Id", id)
        };

        return await ExecuteAsync("dbo.sp_Vehicle", parameters);
    }

    private static Vehicle MapVehicle(SqlDataReader reader) => new()
    {
        Id = GetValue<int>(reader, "Id"),
        RegistrationNumber = GetValue<string>(reader, "RegistrationNumber"),
        Name = GetValue<string>(reader, "Name"),
        Type = GetValue<string>(reader, "Type"),
        Region = GetValue<string>(reader, "Region"),
        MaxLoadKg = GetValue<decimal>(reader, "MaxLoadKg"),
        Odometer = GetValue<decimal>(reader, "Odometer"),
        AcquisitionCost = GetValue<decimal>(reader, "AcquisitionCost"),
        Status = GetValue<string>(reader, "Status")
    };
}

public class DriverRepository : BaseRepository, IDriverRepository
{
    public DriverRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Driver?> GetByIdAsync(int id)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_BY_ID"),
            new SqlParameter("@Id", id)
        };

        return await QuerySingleAsync("dbo.sp_Driver", MapDriver, parameters);
    }

    public async Task<List<Driver>> GetAllAsync(string? status, string? search, string? sortBy, string? sortDir)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_ALL"),
            new SqlParameter("@Status", (object?)status ?? DBNull.Value),
            new SqlParameter("@Search", (object?)search ?? DBNull.Value),
            new SqlParameter("@SortBy", (object?)sortBy ?? DBNull.Value),
            new SqlParameter("@SortDir", (object?)sortDir ?? DBNull.Value)
        };

        return await QueryAsync("dbo.sp_Driver", MapDriver, parameters);
    }

    public async Task<List<Driver>> GetAvailableAsync()
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_AVAILABLE")
        };

        return await QueryAsync("dbo.sp_Driver", MapDriver, parameters);
    }

    public async Task<SpResult> CreateAsync(Driver driver)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "INSERT"),
            new SqlParameter("@Name", driver.Name),
            new SqlParameter("@LicenseNumber", driver.LicenseNumber),
            new SqlParameter("@LicenseCategory", driver.LicenseCategory),
            new SqlParameter("@LicenseExpiry", driver.LicenseExpiry),
            new SqlParameter("@ContactNumber", driver.ContactNumber),
            new SqlParameter("@SafetyScore", driver.SafetyScore),
            new SqlParameter("@Status", string.IsNullOrEmpty(driver.Status) ? "Available" : driver.Status)
        };

        return await ExecuteAsync("dbo.sp_Driver", parameters);
    }

    public async Task<SpResult> UpdateAsync(Driver driver)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "UPDATE"),
            new SqlParameter("@Id", driver.Id),
            new SqlParameter("@Name", driver.Name),
            new SqlParameter("@LicenseNumber", driver.LicenseNumber),
            new SqlParameter("@LicenseCategory", driver.LicenseCategory),
            new SqlParameter("@LicenseExpiry", driver.LicenseExpiry),
            new SqlParameter("@ContactNumber", driver.ContactNumber),
            new SqlParameter("@SafetyScore", driver.SafetyScore),
            new SqlParameter("@Status", (object?)driver.Status ?? DBNull.Value)
        };

        return await ExecuteAsync("dbo.sp_Driver", parameters);
    }

    public async Task<SpResult> UpdateStatusAsync(int id, string status)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "UPDATE_STATUS"),
            new SqlParameter("@Id", id),
            new SqlParameter("@Status", status)
        };

        return await ExecuteAsync("dbo.sp_Driver", parameters);
    }

    private static Driver MapDriver(SqlDataReader reader) => new()
    {
        Id = GetValue<int>(reader, "Id"),
        Name = GetValue<string>(reader, "Name"),
        LicenseNumber = GetValue<string>(reader, "LicenseNumber"),
        LicenseCategory = GetValue<string>(reader, "LicenseCategory"),
        LicenseExpiry = GetValue<DateTime>(reader, "LicenseExpiry"),
        ContactNumber = GetValue<string>(reader, "ContactNumber"),
        SafetyScore = GetValue<int>(reader, "SafetyScore"),
        Status = GetValue<string>(reader, "Status")
    };
}

public class TripRepository : BaseRepository, ITripRepository
{
    public TripRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Trip?> GetByIdAsync(int id)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_BY_ID"),
            new SqlParameter("@Id", id)
        };

        return await QuerySingleAsync("dbo.sp_Trip", MapTrip, parameters);
    }

    public async Task<List<Trip>> GetAllAsync(string? status, int? vehicleId, int? driverId)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_ALL"),
            new SqlParameter("@Status", (object?)status ?? DBNull.Value),
            new SqlParameter("@VehicleId", (object?)vehicleId ?? DBNull.Value),
            new SqlParameter("@DriverId", (object?)driverId ?? DBNull.Value)
        };

        return await QueryAsync("dbo.sp_Trip", MapTrip, parameters);
    }

    public async Task<SpResult> CreateAsync(Trip trip)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "INSERT"),
            new SqlParameter("@Source", trip.Source),
            new SqlParameter("@Destination", trip.Destination),
            new SqlParameter("@VehicleId", trip.VehicleId),
            new SqlParameter("@DriverId", trip.DriverId),
            new SqlParameter("@CargoWeightKg", trip.CargoWeightKg),
            new SqlParameter("@PlannedDistanceKm", trip.PlannedDistanceKm),
            new SqlParameter("@Revenue", trip.Revenue),
            new SqlParameter("@CreatedBy", trip.CreatedBy)
        };

        return await ExecuteAsync("dbo.sp_Trip", parameters);
    }

    public async Task<SpResult> UpdateAsync(Trip trip)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "UPDATE"),
            new SqlParameter("@Id", trip.Id),
            new SqlParameter("@Source", trip.Source),
            new SqlParameter("@Destination", trip.Destination),
            new SqlParameter("@VehicleId", trip.VehicleId),
            new SqlParameter("@DriverId", trip.DriverId),
            new SqlParameter("@CargoWeightKg", trip.CargoWeightKg),
            new SqlParameter("@PlannedDistanceKm", trip.PlannedDistanceKm),
            new SqlParameter("@Revenue", trip.Revenue)
        };

        return await ExecuteAsync("dbo.sp_Trip", parameters);
    }

    public async Task<SpResult> DispatchAsync(int id)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "DISPATCH"),
            new SqlParameter("@Id", id)
        };

        return await ExecuteAsync("dbo.sp_Trip", parameters);
    }

    public async Task<SpResult> CompleteAsync(int id, decimal finalOdometer, decimal fuelConsumedL)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "COMPLETE"),
            new SqlParameter("@Id", id),
            new SqlParameter("@FinalOdometer", finalOdometer),
            new SqlParameter("@FuelConsumedL", fuelConsumedL)
        };

        return await ExecuteAsync("dbo.sp_Trip", parameters);
    }

    public async Task<SpResult> CancelAsync(int id)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "CANCEL"),
            new SqlParameter("@Id", id)
        };

        return await ExecuteAsync("dbo.sp_Trip", parameters);
    }

    private static Trip MapTrip(SqlDataReader reader) => new()
    {
        Id = GetValue<int>(reader, "Id"),
        Source = GetValue<string>(reader, "Source"),
        Destination = GetValue<string>(reader, "Destination"),
        VehicleId = GetValue<int>(reader, "VehicleId"),
        VehicleRegistrationNumber = GetValue<string>(reader, "VehicleRegistrationNumber"),
        DriverId = GetValue<int>(reader, "DriverId"),
        DriverName = GetValue<string>(reader, "DriverName"),
        CargoWeightKg = GetValue<decimal>(reader, "CargoWeightKg"),
        PlannedDistanceKm = GetValue<decimal>(reader, "PlannedDistanceKm"),
        Revenue = GetValue<decimal>(reader, "Revenue"),
        Status = GetValue<string>(reader, "Status"),
        FinalOdometer = GetNullableValue<decimal>(reader, "FinalOdometer"),
        FuelConsumedL = GetNullableValue<decimal>(reader, "FuelConsumedL"),
        ActualDistanceKm = GetNullableValue<decimal>(reader, "ActualDistanceKm"),
        CreatedBy = GetValue<int>(reader, "CreatedBy"),
        CreatedByName = GetValue<string>(reader, "CreatedByName"),
        CreatedAt = GetValue<DateTime>(reader, "CreatedAt"),
        DispatchedAt = GetNullableValue<DateTime>(reader, "DispatchedAt"),
        CompletedAt = GetNullableValue<DateTime>(reader, "CompletedAt")
    };
}

public class MaintenanceRepository : BaseRepository, IMaintenanceRepository
{
    public MaintenanceRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<MaintenanceLog?> GetByIdAsync(int id)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_BY_ID"),
            new SqlParameter("@Id", id)
        };

        return await QuerySingleAsync("dbo.sp_Maintenance", MapLog, parameters);
    }

    public async Task<List<MaintenanceLog>> GetAllAsync()
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_ALL")
        };

        return await QueryAsync("dbo.sp_Maintenance", MapLog, parameters);
    }

    public async Task<SpResult> CreateAsync(int vehicleId, string description, decimal cost)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "INSERT"),
            new SqlParameter("@VehicleId", vehicleId),
            new SqlParameter("@Description", description),
            new SqlParameter("@Cost", cost)
        };

        return await ExecuteAsync("dbo.sp_Maintenance", parameters);
    }

    public async Task<SpResult> CloseAsync(int id)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "CLOSE"),
            new SqlParameter("@Id", id)
        };

        return await ExecuteAsync("dbo.sp_Maintenance", parameters);
    }

    private static MaintenanceLog MapLog(SqlDataReader reader) => new()
    {
        Id = GetValue<int>(reader, "Id"),
        VehicleId = GetValue<int>(reader, "VehicleId"),
        VehicleRegistrationNumber = GetValue<string>(reader, "VehicleRegistrationNumber"),
        Description = GetValue<string>(reader, "Description"),
        Cost = GetValue<decimal>(reader, "Cost"),
        StartDate = GetValue<DateTime>(reader, "StartDate"),
        EndDate = GetNullableValue<DateTime>(reader, "EndDate"),
        Status = GetValue<string>(reader, "Status")
    };
}

public class FuelLogRepository : BaseRepository, IFuelLogRepository
{
    public FuelLogRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<List<FuelLog>> GetAllAsync(int? vehicleId, DateTime? startDate, DateTime? endDate)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_ALL"),
            new SqlParameter("@VehicleId", (object?)vehicleId ?? DBNull.Value),
            new SqlParameter("@StartDate", (object?)startDate ?? DBNull.Value),
            new SqlParameter("@EndDate", (object?)endDate ?? DBNull.Value)
        };

        return await QueryAsync("dbo.sp_FuelLog", reader => new FuelLog
        {
            Id = GetValue<int>(reader, "Id"),
            VehicleId = GetValue<int>(reader, "VehicleId"),
            VehicleRegistrationNumber = GetValue<string>(reader, "VehicleRegistrationNumber"),
            TripId = GetNullableValue<int>(reader, "TripId"),
            Liters = GetValue<decimal>(reader, "Liters"),
            Cost = GetValue<decimal>(reader, "Cost"),
            LogDate = GetValue<DateTime>(reader, "LogDate")
        }, parameters);
    }

    public async Task<SpResult> CreateAsync(FuelLog log)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "INSERT"),
            new SqlParameter("@VehicleId", log.VehicleId),
            new SqlParameter("@TripId", (object?)log.TripId ?? DBNull.Value),
            new SqlParameter("@Liters", log.Liters),
            new SqlParameter("@Cost", log.Cost)
        };

        return await ExecuteAsync("dbo.sp_FuelLog", parameters);
    }
}

public class ExpenseRepository : BaseRepository, IExpenseRepository
{
    public ExpenseRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<List<Expense>> GetAllAsync()
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "GET_ALL")
        };

        return await QueryAsync("dbo.sp_Expense", reader => new Expense
        {
            Id = GetValue<int>(reader, "Id"),
            VehicleId = GetValue<int>(reader, "VehicleId"),
            VehicleRegistrationNumber = GetValue<string>(reader, "VehicleRegistrationNumber"),
            TripId = GetNullableValue<int>(reader, "TripId"),
            Type = GetValue<string>(reader, "Type"),
            Amount = GetValue<decimal>(reader, "Amount"),
            ExpenseDate = GetValue<DateTime>(reader, "ExpenseDate"),
            Notes = GetNullableString(reader, "Notes")
        }, parameters);
    }

    public async Task<SpResult> CreateAsync(Expense expense)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "INSERT"),
            new SqlParameter("@VehicleId", expense.VehicleId),
            new SqlParameter("@TripId", (object?)expense.TripId ?? DBNull.Value),
            new SqlParameter("@Type", expense.Type),
            new SqlParameter("@Amount", expense.Amount),
            new SqlParameter("@Notes", (object?)expense.Notes ?? DBNull.Value)
        };

        return await ExecuteAsync("dbo.sp_Expense", parameters);
    }
}

public class DashboardRepository : BaseRepository, IDashboardRepository
{
    public DashboardRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<DTOs.DashboardResponse> GetKpisAsync(string? vehicleType, string? status, string? region)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "KPI"),
            new SqlParameter("@VehicleType", (object?)vehicleType ?? DBNull.Value),
            new SqlParameter("@Status", (object?)status ?? DBNull.Value),
            new SqlParameter("@Region", (object?)region ?? DBNull.Value)
        };

        var result = await QuerySingleAsync("dbo.sp_Dashboard", reader => new DTOs.DashboardResponse
        {
            ActiveVehicles = GetValue<int>(reader, "ActiveVehicles"),
            AvailableVehicles = GetValue<int>(reader, "AvailableVehicles"),
            VehiclesInMaintenance = GetValue<int>(reader, "VehiclesInMaintenance"),
            ActiveTrips = GetValue<int>(reader, "ActiveTrips"),
            PendingTrips = GetValue<int>(reader, "PendingTrips"),
            DriversOnDuty = GetValue<int>(reader, "DriversOnDuty"),
            FleetUtilizationPercent = Convert.ToDouble(GetValue<object>(reader, "FleetUtilizationPercent"))
        }, parameters);

        return result ?? new DTOs.DashboardResponse();
    }
}

public class ReportRepository : BaseRepository, IReportRepository
{
    public ReportRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<List<DTOs.FuelEfficiencyReportRow>> GetFuelEfficiencyReportAsync()
    {
        var parameters = new[] { new SqlParameter("@Mode", "FUEL_EFFICIENCY") };
        return await QueryAsync("dbo.sp_Report", reader => new DTOs.FuelEfficiencyReportRow
        {
            VehicleId = GetValue<int>(reader, "VehicleId"),
            RegistrationNumber = GetValue<string>(reader, "RegistrationNumber"),
            VehicleName = GetValue<string>(reader, "VehicleName"),
            VehicleType = GetValue<string>(reader, "VehicleType"),
            TotalDistanceKm = GetValue<decimal>(reader, "TotalDistanceKm"),
            TotalFuelConsumedL = GetValue<decimal>(reader, "TotalFuelConsumedL"),
            KmPerLiter = Convert.ToDouble(GetValue<object>(reader, "KmPerLiter"))
        }, parameters);
    }

    public async Task<List<DTOs.UtilizationReportRow>> GetUtilizationReportAsync()
    {
        var parameters = new[] { new SqlParameter("@Mode", "UTILIZATION") };
        return await QueryAsync("dbo.sp_Report", reader => new DTOs.UtilizationReportRow
        {
            VehicleId = GetValue<int>(reader, "VehicleId"),
            RegistrationNumber = GetValue<string>(reader, "RegistrationNumber"),
            VehicleName = GetValue<string>(reader, "VehicleName"),
            CurrentStatus = GetValue<string>(reader, "CurrentStatus"),
            CompletedTripsCount = GetValue<int>(reader, "CompletedTripsCount"),
            TotalDistanceKm = GetValue<decimal>(reader, "TotalDistanceKm"),
            TotalRevenue = GetValue<decimal>(reader, "TotalRevenue")
        }, parameters);
    }

    public async Task<List<DTOs.OperationalCostReportRow>> GetOperationalCostReportAsync()
    {
        var parameters = new[] { new SqlParameter("@Mode", "COST") };
        return await QueryAsync("dbo.sp_Report", reader => new DTOs.OperationalCostReportRow
        {
            VehicleId = GetValue<int>(reader, "VehicleId"),
            RegistrationNumber = GetValue<string>(reader, "RegistrationNumber"),
            VehicleName = GetValue<string>(reader, "VehicleName"),
            TotalFuelCost = GetValue<decimal>(reader, "TotalFuelCost"),
            TotalMaintenanceCost = GetValue<decimal>(reader, "TotalMaintenanceCost"),
            TotalOperationalCost = GetValue<decimal>(reader, "TotalOperationalCost"),
            OtherExpensesCost = GetValue<decimal>(reader, "OtherExpensesCost")
        }, parameters);
    }

    public async Task<List<DTOs.RoiReportRow>> GetRoiReportAsync()
    {
        var parameters = new[] { new SqlParameter("@Mode", "ROI") };
        return await QueryAsync("dbo.sp_Report", reader => new DTOs.RoiReportRow
        {
            VehicleId = GetValue<int>(reader, "VehicleId"),
            RegistrationNumber = GetValue<string>(reader, "RegistrationNumber"),
            VehicleName = GetValue<string>(reader, "VehicleName"),
            AcquisitionCost = GetValue<decimal>(reader, "AcquisitionCost"),
            TotalRevenue = GetValue<decimal>(reader, "TotalRevenue"),
            TotalFuelCost = GetValue<decimal>(reader, "TotalFuelCost"),
            TotalMaintenanceCost = GetValue<decimal>(reader, "TotalMaintenanceCost"),
            TotalOperationalCost = GetValue<decimal>(reader, "TotalOperationalCost"),
            RoiRatio = Convert.ToDouble(GetValue<object>(reader, "RoiRatio"))
        }, parameters);
    }

    public async Task<DTOs.VehicleAnalyticsResponse?> GetVehicleAnalyticsAsync(int vehicleId)
    {
        var parameters = new[]
        {
            new SqlParameter("@Mode", "VEHICLE_ANALYTICS"),
            new SqlParameter("@VehicleId", vehicleId)
        };

        return await QuerySingleAsync("dbo.sp_Report", reader => new DTOs.VehicleAnalyticsResponse
        {
            VehicleId = GetValue<int>(reader, "VehicleId"),
            RegistrationNumber = GetValue<string>(reader, "RegistrationNumber"),
            VehicleName = GetValue<string>(reader, "VehicleName"),
            VehicleType = GetValue<string>(reader, "VehicleType"),
            CurrentStatus = GetValue<string>(reader, "CurrentStatus"),
            Odometer = GetValue<decimal>(reader, "Odometer"),
            AcquisitionCost = GetValue<decimal>(reader, "AcquisitionCost"),
            TotalRevenue = GetValue<decimal>(reader, "TotalRevenue"),
            TotalDistanceKm = GetValue<decimal>(reader, "TotalDistanceKm"),
            TotalFuelCost = GetValue<decimal>(reader, "TotalFuelCost"),
            TotalMaintenanceCost = GetValue<decimal>(reader, "TotalMaintenanceCost"),
            OtherExpensesCost = GetValue<decimal>(reader, "OtherExpensesCost"),
            TotalOperationalCost = GetValue<decimal>(reader, "TotalOperationalCost"),
            FuelEfficiencyKmPerL = Convert.ToDouble(GetValue<object>(reader, "FuelEfficiencyKmPerL")),
            RoiRatio = Convert.ToDouble(GetValue<object>(reader, "RoiRatio"))
        }, parameters);
    }
}

