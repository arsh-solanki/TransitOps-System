using TransitOps.Api.Domain.Enums;
using TransitOps.Api.Exceptions;

namespace TransitOps.Api.Domain.Rules;

public static class MaintenanceRules
{
    public static void ValidateCanClose(string currentStatus)
    {
        if (string.Equals(currentStatus, "Closed", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Maintenance log is already closed.");
    }

    public static string DetermineVehicleStatusOnClose(string vehicleStatus)
    {
        // Closing maintenance restores vehicle to Available UNLESS it is Retired
        if (string.Equals(vehicleStatus, VehicleStatus.Retired.ToString(), StringComparison.OrdinalIgnoreCase))
            return VehicleStatus.Retired.ToString();

        return VehicleStatus.Available.ToString();
    }
}

