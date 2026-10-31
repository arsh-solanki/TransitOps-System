using System.ComponentModel.DataAnnotations;

namespace TransitOps.Api.DTOs;

// --- Auth DTOs ---
public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

// --- Vehicle DTOs ---
public class VehicleCreateRequest
{
    [Required, StringLength(50)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Region { get; set; } = string.Empty;

    [Range(0.01, 100000.00)]
    public decimal MaxLoadKg { get; set; }

    [Range(0, 10000000.00)]
    public decimal Odometer { get; set; }

    [Range(0.01, 100000000.00)]
    public decimal AcquisitionCost { get; set; }

    public string? Status { get; set; } = "Available";
}

public class VehicleUpdateRequest
{
    [Required, StringLength(50)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Region { get; set; } = string.Empty;

    [Range(0.01, 100000.00)]
    public decimal MaxLoadKg { get; set; }

    [Range(0, 10000000.00)]
    public decimal Odometer { get; set; }

    [Range(0.01, 100000000.00)]
    public decimal AcquisitionCost { get; set; }

    public string? Status { get; set; }
}

public class VehicleResponse
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public decimal MaxLoadKg { get; set; }
    public decimal Odometer { get; set; }
    public decimal AcquisitionCost { get; set; }
    public string Status { get; set; } = string.Empty;
}

// --- Driver DTOs ---
public class DriverCreateRequest
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string LicenseCategory { get; set; } = string.Empty;

    [Required]
    public DateTime LicenseExpiry { get; set; }

    [Required, StringLength(30)]
    public string ContactNumber { get; set; } = string.Empty;

    [Range(0, 100)]
    public int SafetyScore { get; set; } = 100;

    public string? Status { get; set; } = "Available";
}

public class DriverUpdateRequest
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string LicenseCategory { get; set; } = string.Empty;

    [Required]
    public DateTime LicenseExpiry { get; set; }

    [Required, StringLength(30)]
    public string ContactNumber { get; set; } = string.Empty;

    [Range(0, 100)]
    public int SafetyScore { get; set; }

    public string? Status { get; set; }
}

public class DriverStatusUpdateRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class DriverResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseCategory { get; set; } = string.Empty;
    public DateTime LicenseExpiry { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public int SafetyScore { get; set; }
    public string Status { get; set; } = string.Empty;
}

// --- Trip DTOs ---
public class TripCreateRequest
{
    [Required, StringLength(150)]
    public string Source { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Destination { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int DriverId { get; set; }

    [Range(0.01, 100000.00)]
    public decimal CargoWeightKg { get; set; }

    [Range(0.01, 100000.00)]
    public decimal PlannedDistanceKm { get; set; }

    [Range(0, 10000000.00)]
    public decimal Revenue { get; set; }
}

public class TripUpdateRequest
{
    [Required, StringLength(150)]
    public string Source { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Destination { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int DriverId { get; set; }

    [Range(0.01, 100000.00)]
    public decimal CargoWeightKg { get; set; }

    [Range(0.01, 100000.00)]
    public decimal PlannedDistanceKm { get; set; }

    [Range(0, 10000000.00)]
    public decimal Revenue { get; set; }
}

public class TripCompleteRequest
{
    [Required, Range(0, 10000000.00)]
    public decimal FinalOdometer { get; set; }

    [Required, Range(0.01, 100000.00)]
    public decimal FuelConsumedL { get; set; }
}

public class TripResponse
{
    public int Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public decimal CargoWeightKg { get; set; }
    public decimal PlannedDistanceKm { get; set; }
    public decimal Revenue { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? FinalOdometer { get; set; }
    public decimal? FuelConsumedL { get; set; }
    public decimal? ActualDistanceKm { get; set; }
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// --- Maintenance DTOs ---
public class MaintenanceCreateRequest
{
    [Required, Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Required, StringLength(255)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 10000000.00)]
    public decimal Cost { get; set; }
}

public class MaintenanceResponse
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

// --- Fuel Log DTOs ---
public class FuelLogCreateRequest
{
    [Required, Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    public int? TripId { get; set; }

    [Range(0.01, 100000.00)]
    public decimal Liters { get; set; }

    [Range(0, 10000000.00)]
    public decimal Cost { get; set; }
}

public class FuelLogResponse
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public int? TripId { get; set; }
    public decimal Liters { get; set; }
    public decimal Cost { get; set; }
    public DateTime LogDate { get; set; }
}

// --- Expense DTOs ---
public class ExpenseCreateRequest
{
    [Required, Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    public int? TripId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Range(0.01, 10000000.00)]
    public decimal Amount { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class ExpenseResponse
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public int? TripId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Notes { get; set; }
}

// --- Dashboard DTOs ---
public class DashboardResponse
{
    public int ActiveVehicles { get; set; }
    public int AvailableVehicles { get; set; }
    public int VehiclesInMaintenance { get; set; }
    public int ActiveTrips { get; set; }
    public int PendingTrips { get; set; }
    public int DriversOnDuty { get; set; }
    public double FleetUtilizationPercent { get; set; }
}

// --- Report DTOs ---
public class FuelEfficiencyReportRow
{
    public int VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public decimal TotalDistanceKm { get; set; }
    public decimal TotalFuelConsumedL { get; set; }
    public double KmPerLiter { get; set; }
}

public class UtilizationReportRow
{
    public int VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public int CompletedTripsCount { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class OperationalCostReportRow
{
    public int VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public decimal TotalFuelCost { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
    public decimal TotalOperationalCost { get; set; }
    public decimal OtherExpensesCost { get; set; }
}

public class RoiReportRow
{
    public int VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public decimal AcquisitionCost { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalFuelCost { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
    public decimal TotalOperationalCost { get; set; }
    public double RoiRatio { get; set; }
}

public class VehicleAnalyticsResponse
{
    public int VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public decimal Odometer { get; set; }
    public decimal AcquisitionCost { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public decimal TotalFuelCost { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
    public decimal OtherExpensesCost { get; set; }
    public decimal TotalOperationalCost { get; set; }
    public double FuelEfficiencyKmPerL { get; set; }
    public double RoiRatio { get; set; }
}

