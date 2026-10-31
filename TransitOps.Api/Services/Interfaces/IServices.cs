using TransitOps.Api.DTOs;

namespace TransitOps.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}

public interface IVehicleService
{
    Task<VehicleResponse> GetByIdAsync(int id);
    Task<List<VehicleResponse>> GetAllAsync(string? type, string? status, string? region, string? search, string? sortBy, string? sortDir);
    Task<List<VehicleResponse>> GetAvailableAsync();
    Task<VehicleResponse> CreateAsync(VehicleCreateRequest request);
    Task UpdateAsync(int id, VehicleUpdateRequest request);
    Task DeleteAsync(int id);
}

public interface IDriverService
{
    Task<DriverResponse> GetByIdAsync(int id);
    Task<List<DriverResponse>> GetAllAsync(string? status, string? search, string? sortBy, string? sortDir);
    Task<List<DriverResponse>> GetAvailableAsync();
    Task<DriverResponse> CreateAsync(DriverCreateRequest request);
    Task UpdateAsync(int id, DriverUpdateRequest request);
    Task UpdateStatusAsync(int id, string status);
}

public interface ITripService
{
    Task<TripResponse> GetByIdAsync(int id);
    Task<List<TripResponse>> GetAllAsync(string? status, int? vehicleId, int? driverId);
    Task<TripResponse> CreateAsync(TripCreateRequest request, int userId);
    Task UpdateAsync(int id, TripUpdateRequest request);
    Task DispatchAsync(int id);
    Task CompleteAsync(int id, TripCompleteRequest request);
    Task CancelAsync(int id);
}

public interface IMaintenanceService
{
    Task<MaintenanceResponse> GetByIdAsync(int id);
    Task<List<MaintenanceResponse>> GetAllAsync();
    Task<MaintenanceResponse> CreateAsync(MaintenanceCreateRequest request);
    Task CloseAsync(int id);
}

public interface IFuelLogService
{
    Task<List<FuelLogResponse>> GetAllAsync(int? vehicleId, DateTime? startDate, DateTime? endDate);
    Task<FuelLogResponse> CreateAsync(FuelLogCreateRequest request);
}

public interface IExpenseService
{
    Task<List<ExpenseResponse>> GetAllAsync();
    Task<ExpenseResponse> CreateAsync(ExpenseCreateRequest request);
}

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardKpisAsync(string? vehicleType, string? status, string? region);
}

public interface IReportService
{
    Task<List<FuelEfficiencyReportRow>> GetFuelEfficiencyAsync();
    Task<List<UtilizationReportRow>> GetUtilizationAsync();
    Task<List<OperationalCostReportRow>> GetOperationalCostAsync();
    Task<List<RoiReportRow>> GetRoiAsync();
    Task<VehicleAnalyticsResponse> GetVehicleAnalyticsAsync(int vehicleId);
    byte[] GenerateCsv<T>(IEnumerable<T> data);
}

