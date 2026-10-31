using TransitOps.Api.Base;
using TransitOps.Api.Models;

namespace TransitOps.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<SpResult> RegisterAsync(string fullName, string email, string passwordHash, int roleId);
}

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(int id);
    Task<List<Vehicle>> GetAllAsync(string? type, string? status, string? region, string? search, string? sortBy, string? sortDir);
    Task<List<Vehicle>> GetAvailableAsync();
    Task<SpResult> CreateAsync(Vehicle vehicle);
    Task<SpResult> UpdateAsync(Vehicle vehicle);
    Task<SpResult> DeleteAsync(int id);
}

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(int id);
    Task<List<Driver>> GetAllAsync(string? status, string? search, string? sortBy, string? sortDir);
    Task<List<Driver>> GetAvailableAsync();
    Task<SpResult> CreateAsync(Driver driver);
    Task<SpResult> UpdateAsync(Driver driver);
    Task<SpResult> UpdateStatusAsync(int id, string status);
}

public interface ITripRepository
{
    Task<Trip?> GetByIdAsync(int id);
    Task<List<Trip>> GetAllAsync(string? status, int? vehicleId, int? driverId);
    Task<SpResult> CreateAsync(Trip trip);
    Task<SpResult> UpdateAsync(Trip trip);
    Task<SpResult> DispatchAsync(int id);
    Task<SpResult> CompleteAsync(int id, decimal finalOdometer, decimal fuelConsumedL);
    Task<SpResult> CancelAsync(int id);
}

public interface IMaintenanceRepository
{
    Task<MaintenanceLog?> GetByIdAsync(int id);
    Task<List<MaintenanceLog>> GetAllAsync();
    Task<SpResult> CreateAsync(int vehicleId, string description, decimal cost);
    Task<SpResult> CloseAsync(int id);
}

public interface IFuelLogRepository
{
    Task<List<FuelLog>> GetAllAsync(int? vehicleId, DateTime? startDate, DateTime? endDate);
    Task<SpResult> CreateAsync(FuelLog log);
}

public interface IExpenseRepository
{
    Task<List<Expense>> GetAllAsync();
    Task<SpResult> CreateAsync(Expense expense);
}

public interface IDashboardRepository
{
    Task<DTOs.DashboardResponse> GetKpisAsync(string? vehicleType, string? status, string? region);
}

public interface IReportRepository
{
    Task<List<DTOs.FuelEfficiencyReportRow>> GetFuelEfficiencyReportAsync();
    Task<List<DTOs.UtilizationReportRow>> GetUtilizationReportAsync();
    Task<List<DTOs.OperationalCostReportRow>> GetOperationalCostReportAsync();
    Task<List<DTOs.RoiReportRow>> GetRoiReportAsync();
    Task<DTOs.VehicleAnalyticsResponse?> GetVehicleAnalyticsAsync(int vehicleId);
}

