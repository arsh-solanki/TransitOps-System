namespace TransitOps.Api.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Vehicle
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
    public byte[]? RowVer { get; set; }
}

public class Driver
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseCategory { get; set; } = string.Empty;
    public DateTime LicenseExpiry { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public int SafetyScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public byte[]? RowVer { get; set; }
}

public class Trip
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
    public byte[]? RowVer { get; set; }
}

public class MaintenanceLog
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

public class FuelLog
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public int? TripId { get; set; }
    public decimal Liters { get; set; }
    public decimal Cost { get; set; }
    public DateTime LogDate { get; set; }
}

public class Expense
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

