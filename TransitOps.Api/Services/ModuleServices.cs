using System.Text;
using TransitOps.Api.Domain.Rules;
using TransitOps.Api.DTOs;
using TransitOps.Api.Exceptions;
using TransitOps.Api.Models;
using TransitOps.Api.Repositories.Interfaces;
using TransitOps.Api.Services.Interfaces;

namespace TransitOps.Api.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<VehicleResponse> GetByIdAsync(int id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null)
            throw new NotFoundException($"Vehicle with ID {id} not found.");

        return MapToResponse(vehicle);
    }

    public async Task<List<VehicleResponse>> GetAllAsync(string? type, string? status, string? region, string? search, string? sortBy, string? sortDir)
    {
        var vehicles = await _vehicleRepository.GetAllAsync(type, status, region, search, sortBy, sortDir);
        return vehicles.Select(MapToResponse).ToList();
    }

    public async Task<List<VehicleResponse>> GetAvailableAsync()
    {
        var vehicles = await _vehicleRepository.GetAvailableAsync();
        return vehicles.Select(MapToResponse).ToList();
    }

    public async Task<VehicleResponse> CreateAsync(VehicleCreateRequest request)
    {
        var entity = new Vehicle
        {
            RegistrationNumber = request.RegistrationNumber.Trim(),
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            Region = request.Region.Trim(),
            MaxLoadKg = request.MaxLoadKg,
            Odometer = request.Odometer,
            AcquisitionCost = request.AcquisitionCost,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Available" : request.Status.Trim()
        };

        var spResult = await _vehicleRepository.CreateAsync(entity);
        spResult.ThrowIfFailed();

        entity.Id = spResult.NewId;
        return MapToResponse(entity);
    }

    public async Task UpdateAsync(int id, VehicleUpdateRequest request)
    {
        var existing = await _vehicleRepository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException($"Vehicle with ID {id} not found.");

        var entity = new Vehicle
        {
            Id = id,
            RegistrationNumber = request.RegistrationNumber.Trim(),
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            Region = request.Region.Trim(),
            MaxLoadKg = request.MaxLoadKg,
            Odometer = request.Odometer,
            AcquisitionCost = request.AcquisitionCost,
            Status = string.IsNullOrWhiteSpace(request.Status) ? existing.Status : request.Status.Trim()
        };

        var spResult = await _vehicleRepository.UpdateAsync(entity);
        spResult.ThrowIfFailed();
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await _vehicleRepository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException($"Vehicle with ID {id} not found.");

        var spResult = await _vehicleRepository.DeleteAsync(id);
        spResult.ThrowIfFailed();
    }

    private static VehicleResponse MapToResponse(Vehicle v) => new()
    {
        Id = v.Id,
        RegistrationNumber = v.RegistrationNumber,
        Name = v.Name,
        Type = v.Type,
        Region = v.Region,
        MaxLoadKg = v.MaxLoadKg,
        Odometer = v.Odometer,
        AcquisitionCost = v.AcquisitionCost,
        Status = v.Status
    };
}

public class DriverService : IDriverService
{
    private readonly IDriverRepository _driverRepository;

    public DriverService(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    public async Task<DriverResponse> GetByIdAsync(int id)
    {
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver == null)
            throw new NotFoundException($"Driver with ID {id} not found.");

        return MapToResponse(driver);
    }

    public async Task<List<DriverResponse>> GetAllAsync(string? status, string? search, string? sortBy, string? sortDir)
    {
        var drivers = await _driverRepository.GetAllAsync(status, search, sortBy, sortDir);
        return drivers.Select(MapToResponse).ToList();
    }

    public async Task<List<DriverResponse>> GetAvailableAsync()
    {
        var drivers = await _driverRepository.GetAvailableAsync();
        return drivers.Select(MapToResponse).ToList();
    }

    public async Task<DriverResponse> CreateAsync(DriverCreateRequest request)
    {
        var entity = new Driver
        {
            Name = request.Name.Trim(),
            LicenseNumber = request.LicenseNumber.Trim(),
            LicenseCategory = request.LicenseCategory.Trim(),
            LicenseExpiry = request.LicenseExpiry,
            ContactNumber = request.ContactNumber.Trim(),
            SafetyScore = request.SafetyScore,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Available" : request.Status.Trim()
        };

        var spResult = await _driverRepository.CreateAsync(entity);
        spResult.ThrowIfFailed();

        entity.Id = spResult.NewId;
        return MapToResponse(entity);
    }

    public async Task UpdateAsync(int id, DriverUpdateRequest request)
    {
        var existing = await _driverRepository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException($"Driver with ID {id} not found.");

        var entity = new Driver
        {
            Id = id,
            Name = request.Name.Trim(),
            LicenseNumber = request.LicenseNumber.Trim(),
            LicenseCategory = request.LicenseCategory.Trim(),
            LicenseExpiry = request.LicenseExpiry,
            ContactNumber = request.ContactNumber.Trim(),
            SafetyScore = request.SafetyScore,
            Status = string.IsNullOrWhiteSpace(request.Status) ? existing.Status : request.Status.Trim()
        };

        var spResult = await _driverRepository.UpdateAsync(entity);
        spResult.ThrowIfFailed();
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        var existing = await _driverRepository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException($"Driver with ID {id} not found.");

        var spResult = await _driverRepository.UpdateStatusAsync(id, status);
        spResult.ThrowIfFailed();
    }

    private static DriverResponse MapToResponse(Driver d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        LicenseNumber = d.LicenseNumber,
        LicenseCategory = d.LicenseCategory,
        LicenseExpiry = d.LicenseExpiry,
        ContactNumber = d.ContactNumber,
        SafetyScore = d.SafetyScore,
        Status = d.Status
    };
}

public class TripService : ITripService
{
    private readonly ITripRepository _tripRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDriverRepository _driverRepository;

    public TripService(ITripRepository tripRepository, IVehicleRepository vehicleRepository, IDriverRepository driverRepository)
    {
        _tripRepository = tripRepository;
        _vehicleRepository = vehicleRepository;
        _driverRepository = driverRepository;
    }

    public async Task<TripResponse> GetByIdAsync(int id)
    {
        var trip = await _tripRepository.GetByIdAsync(id);
        if (trip == null)
            throw new NotFoundException($"Trip with ID {id} not found.");

        return MapToResponse(trip);
    }

    public async Task<List<TripResponse>> GetAllAsync(string? status, int? vehicleId, int? driverId)
    {
        var trips = await _tripRepository.GetAllAsync(status, vehicleId, driverId);
        return trips.Select(MapToResponse).ToList();
    }

    public async Task<TripResponse> CreateAsync(TripCreateRequest request, int userId)
    {
        // Pure domain validation in C#
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
            throw new NotFoundException($"Vehicle with ID {request.VehicleId} not found.");

        VehicleRules.ValidateForAssignment(vehicle.Status);
        VehicleRules.ValidateCapacity(request.CargoWeightKg, vehicle.MaxLoadKg);

        var driver = await _driverRepository.GetByIdAsync(request.DriverId);
        if (driver == null)
            throw new NotFoundException($"Driver with ID {request.DriverId} not found.");

        DriverRules.ValidateForAssignment(driver.Status, driver.LicenseExpiry, DateTime.UtcNow);

        var trip = new Trip
        {
            Source = request.Source.Trim(),
            Destination = request.Destination.Trim(),
            VehicleId = request.VehicleId,
            DriverId = request.DriverId,
            CargoWeightKg = request.CargoWeightKg,
            PlannedDistanceKm = request.PlannedDistanceKm,
            Revenue = request.Revenue,
            CreatedBy = userId
        };

        var spResult = await _tripRepository.CreateAsync(trip);
        spResult.ThrowIfFailed();

        var created = await _tripRepository.GetByIdAsync(spResult.NewId);
        return MapToResponse(created!);
    }

    public async Task UpdateAsync(int id, TripUpdateRequest request)
    {
        var trip = await _tripRepository.GetByIdAsync(id);
        if (trip == null)
            throw new NotFoundException($"Trip with ID {id} not found.");

        if (!string.Equals(trip.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Only trips in Draft status can be edited.");

        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
            throw new NotFoundException($"Vehicle with ID {request.VehicleId} not found.");

        VehicleRules.ValidateCapacity(request.CargoWeightKg, vehicle.MaxLoadKg);

        var updateTrip = new Trip
        {
            Id = id,
            Source = request.Source.Trim(),
            Destination = request.Destination.Trim(),
            VehicleId = request.VehicleId,
            DriverId = request.DriverId,
            CargoWeightKg = request.CargoWeightKg,
            PlannedDistanceKm = request.PlannedDistanceKm,
            Revenue = request.Revenue
        };

        var spResult = await _tripRepository.UpdateAsync(updateTrip);
        spResult.ThrowIfFailed();
    }

    public async Task DispatchAsync(int id)
    {
        var trip = await _tripRepository.GetByIdAsync(id);
        if (trip == null)
            throw new NotFoundException($"Trip with ID {id} not found.");

        TripRules.ValidateDispatchTransition(trip.Status);

        var vehicle = await _vehicleRepository.GetByIdAsync(trip.VehicleId);
        if (vehicle != null)
        {
            VehicleRules.ValidateForAssignment(vehicle.Status);
            VehicleRules.ValidateCapacity(trip.CargoWeightKg, vehicle.MaxLoadKg);
        }

        var driver = await _driverRepository.GetByIdAsync(trip.DriverId);
        if (driver != null)
        {
            DriverRules.ValidateForAssignment(driver.Status, driver.LicenseExpiry, DateTime.UtcNow);
        }

        var spResult = await _tripRepository.DispatchAsync(id);
        spResult.ThrowIfFailed();
    }

    public async Task CompleteAsync(int id, TripCompleteRequest request)
    {
        var trip = await _tripRepository.GetByIdAsync(id);
        if (trip == null)
            throw new NotFoundException($"Trip with ID {id} not found.");

        var vehicle = await _vehicleRepository.GetByIdAsync(trip.VehicleId);
        if (vehicle == null)
            throw new NotFoundException($"Associated vehicle with ID {trip.VehicleId} not found.");

        TripRules.ValidateCompleteTransition(trip.Status, vehicle.Odometer, request.FinalOdometer, request.FuelConsumedL);

        var spResult = await _tripRepository.CompleteAsync(id, request.FinalOdometer, request.FuelConsumedL);
        spResult.ThrowIfFailed();
    }

    public async Task CancelAsync(int id)
    {
        var trip = await _tripRepository.GetByIdAsync(id);
        if (trip == null)
            throw new NotFoundException($"Trip with ID {id} not found.");

        TripRules.ValidateCancelTransition(trip.Status);

        var spResult = await _tripRepository.CancelAsync(id);
        spResult.ThrowIfFailed();
    }

    private static TripResponse MapToResponse(Trip t) => new()
    {
        Id = t.Id,
        Source = t.Source,
        Destination = t.Destination,
        VehicleId = t.VehicleId,
        VehicleRegistrationNumber = t.VehicleRegistrationNumber,
        DriverId = t.DriverId,
        DriverName = t.DriverName,
        CargoWeightKg = t.CargoWeightKg,
        PlannedDistanceKm = t.PlannedDistanceKm,
        Revenue = t.Revenue,
        Status = t.Status,
        FinalOdometer = t.FinalOdometer,
        FuelConsumedL = t.FuelConsumedL,
        ActualDistanceKm = t.ActualDistanceKm,
        CreatedBy = t.CreatedBy,
        CreatedByName = t.CreatedByName,
        CreatedAt = t.CreatedAt,
        DispatchedAt = t.DispatchedAt,
        CompletedAt = t.CompletedAt
    };
}

public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public MaintenanceService(IMaintenanceRepository maintenanceRepository, IVehicleRepository vehicleRepository)
    {
        _maintenanceRepository = maintenanceRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<MaintenanceResponse> GetByIdAsync(int id)
    {
        var log = await _maintenanceRepository.GetByIdAsync(id);
        if (log == null)
            throw new NotFoundException($"Maintenance record with ID {id} not found.");

        return MapToResponse(log);
    }

    public async Task<List<MaintenanceResponse>> GetAllAsync()
    {
        var logs = await _maintenanceRepository.GetAllAsync();
        return logs.Select(MapToResponse).ToList();
    }

    public async Task<MaintenanceResponse> CreateAsync(MaintenanceCreateRequest request)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
            throw new NotFoundException($"Vehicle with ID {request.VehicleId} not found.");

        VehicleRules.ValidateForMaintenance(vehicle.Status);

        var spResult = await _maintenanceRepository.CreateAsync(request.VehicleId, request.Description.Trim(), request.Cost);
        spResult.ThrowIfFailed();

        var created = await _maintenanceRepository.GetByIdAsync(spResult.NewId);
        return MapToResponse(created!);
    }

    public async Task CloseAsync(int id)
    {
        var log = await _maintenanceRepository.GetByIdAsync(id);
        if (log == null)
            throw new NotFoundException($"Maintenance record with ID {id} not found.");

        MaintenanceRules.ValidateCanClose(log.Status);

        var spResult = await _maintenanceRepository.CloseAsync(id);
        spResult.ThrowIfFailed();
    }

    private static MaintenanceResponse MapToResponse(MaintenanceLog m) => new()
    {
        Id = m.Id,
        VehicleId = m.VehicleId,
        VehicleRegistrationNumber = m.VehicleRegistrationNumber,
        Description = m.Description,
        Cost = m.Cost,
        StartDate = m.StartDate,
        EndDate = m.EndDate,
        Status = m.Status
    };
}

public class FuelLogService : IFuelLogService
{
    private readonly IFuelLogRepository _fuelLogRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public FuelLogService(IFuelLogRepository fuelLogRepository, IVehicleRepository vehicleRepository)
    {
        _fuelLogRepository = fuelLogRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<List<FuelLogResponse>> GetAllAsync(int? vehicleId, DateTime? startDate, DateTime? endDate)
    {
        var logs = await _fuelLogRepository.GetAllAsync(vehicleId, startDate, endDate);
        return logs.Select(MapToResponse).ToList();
    }

    public async Task<FuelLogResponse> CreateAsync(FuelLogCreateRequest request)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
            throw new NotFoundException($"Vehicle with ID {request.VehicleId} not found.");

        var log = new FuelLog
        {
            VehicleId = request.VehicleId,
            TripId = request.TripId,
            Liters = request.Liters,
            Cost = request.Cost
        };

        var spResult = await _fuelLogRepository.CreateAsync(log);
        spResult.ThrowIfFailed();

        log.Id = spResult.NewId;
        log.VehicleRegistrationNumber = vehicle.RegistrationNumber;
        log.LogDate = DateTime.UtcNow;

        return MapToResponse(log);
    }

    private static FuelLogResponse MapToResponse(FuelLog f) => new()
    {
        Id = f.Id,
        VehicleId = f.VehicleId,
        VehicleRegistrationNumber = f.VehicleRegistrationNumber,
        TripId = f.TripId,
        Liters = f.Liters,
        Cost = f.Cost,
        LogDate = f.LogDate
    };
}

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public ExpenseService(IExpenseRepository expenseRepository, IVehicleRepository vehicleRepository)
    {
        _expenseRepository = expenseRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<List<ExpenseResponse>> GetAllAsync()
    {
        var expenses = await _expenseRepository.GetAllAsync();
        return expenses.Select(MapToResponse).ToList();
    }

    public async Task<ExpenseResponse> CreateAsync(ExpenseCreateRequest request)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
            throw new NotFoundException($"Vehicle with ID {request.VehicleId} not found.");

        var expense = new Expense
        {
            VehicleId = request.VehicleId,
            TripId = request.TripId,
            Type = request.Type.Trim(),
            Amount = request.Amount,
            Notes = request.Notes?.Trim()
        };

        var spResult = await _expenseRepository.CreateAsync(expense);
        spResult.ThrowIfFailed();

        expense.Id = spResult.NewId;
        expense.VehicleRegistrationNumber = vehicle.RegistrationNumber;
        expense.ExpenseDate = DateTime.UtcNow;

        return MapToResponse(expense);
    }

    private static ExpenseResponse MapToResponse(Expense e) => new()
    {
        Id = e.Id,
        VehicleId = e.VehicleId,
        VehicleRegistrationNumber = e.VehicleRegistrationNumber,
        TripId = e.TripId,
        Type = e.Type,
        Amount = e.Amount,
        ExpenseDate = e.ExpenseDate,
        Notes = e.Notes
    };
}

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardResponse> GetDashboardKpisAsync(string? vehicleType, string? status, string? region)
    {
        return await _dashboardRepository.GetKpisAsync(vehicleType, status, region);
    }
}

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<List<FuelEfficiencyReportRow>> GetFuelEfficiencyAsync() =>
        await _reportRepository.GetFuelEfficiencyReportAsync();

    public async Task<List<UtilizationReportRow>> GetUtilizationAsync() =>
        await _reportRepository.GetUtilizationReportAsync();

    public async Task<List<OperationalCostReportRow>> GetOperationalCostAsync() =>
        await _reportRepository.GetOperationalCostReportAsync();

    public async Task<List<RoiReportRow>> GetRoiAsync() =>
        await _reportRepository.GetRoiReportAsync();

    public async Task<VehicleAnalyticsResponse> GetVehicleAnalyticsAsync(int vehicleId)
    {
        var result = await _reportRepository.GetVehicleAnalyticsAsync(vehicleId);
        if (result == null)
            throw new NotFoundException($"Vehicle with ID {vehicleId} not found.");
        return result;
    }

    public byte[] GenerateCsv<T>(IEnumerable<T> data)
    {
        var properties = typeof(T).GetProperties();
        var sb = new StringBuilder();

        // Header
        sb.AppendLine(string.Join(",", properties.Select(p => $"\"{p.Name}\"")));

        // Rows
        foreach (var item in data)
        {
            var line = string.Join(",", properties.Select(p =>
            {
                var val = p.GetValue(item, null);
                if (val == null) return "\"\"";
                var strVal = val.ToString()?.Replace("\"", "\"\"") ?? "";
                return $"\"{strVal}\"";
            }));
            sb.AppendLine(line);
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}

