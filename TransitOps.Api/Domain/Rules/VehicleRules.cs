using TransitOps.Api.Domain.Enums;
using TransitOps.Api.Exceptions;

namespace TransitOps.Api.Domain.Rules;

public static class VehicleRules
{
    public static void ValidateForAssignment(string status)
    {
        if (string.Equals(status, VehicleStatus.Retired.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Retired vehicles cannot be assigned or dispatched.");

        if (string.Equals(status, VehicleStatus.InShop.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("In Shop vehicles cannot be assigned or dispatched.");

        if (string.Equals(status, VehicleStatus.OnTrip.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Vehicle is currently On Trip and cannot be assigned to another trip.");

        if (!string.Equals(status, VehicleStatus.Available.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException($"Vehicle status '{status}' is not Available for assignment.");
    }

    public static void ValidateCapacity(decimal cargoWeightKg, decimal maxLoadKg)
    {
        if (cargoWeightKg <= 0)
            throw new BusinessRuleException("Cargo weight must be greater than zero.");

        if (cargoWeightKg > maxLoadKg)
            throw new BusinessRuleException($"Cargo weight ({cargoWeightKg} kg) exceeds vehicle max load capacity ({maxLoadKg} kg).");
    }

    public static void ValidateForMaintenance(string vehicleStatus)
    {
        if (string.Equals(vehicleStatus, VehicleStatus.OnTrip.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Cannot place a vehicle in maintenance while it is On Trip.");

        if (string.Equals(vehicleStatus, VehicleStatus.Retired.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Cannot place a Retired vehicle in maintenance.");

        if (string.Equals(vehicleStatus, VehicleStatus.InShop.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new ConflictException("Vehicle is already In Shop under active maintenance.");
    }
}

