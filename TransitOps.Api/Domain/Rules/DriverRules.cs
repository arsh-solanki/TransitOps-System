using TransitOps.Api.Domain.Enums;
using TransitOps.Api.Exceptions;

namespace TransitOps.Api.Domain.Rules;

public static class DriverRules
{
    public static void ValidateForAssignment(string status, DateTime licenseExpiry, DateTime currentDate)
    {
        if (string.Equals(status, DriverStatus.Suspended.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Suspended drivers cannot be assigned or dispatched.");

        if (string.Equals(status, DriverStatus.OffDuty.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Off Duty drivers cannot be assigned to a trip.");

        if (string.Equals(status, DriverStatus.OnTrip.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Driver is currently On Trip and cannot be assigned to another trip.");

        if (!string.Equals(status, DriverStatus.Available.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException($"Driver status '{status}' is not Available for assignment.");

        ValidateLicenseValidity(licenseExpiry, currentDate);
    }

    public static void ValidateLicenseValidity(DateTime licenseExpiry, DateTime currentDate)
    {
        // Valid through the expiry date (inclusive)
        if (licenseExpiry.Date < currentDate.Date)
            throw new BusinessRuleException($"Driver license has expired on {licenseExpiry:yyyy-MM-dd}.");
    }
}

