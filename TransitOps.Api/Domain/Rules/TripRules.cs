using TransitOps.Api.Domain.Enums;
using TransitOps.Api.Exceptions;

namespace TransitOps.Api.Domain.Rules;

public static class TripRules
{
    public static void ValidateDispatchTransition(string currentStatus)
    {
        if (!string.Equals(currentStatus, TripStatus.Draft.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException($"Cannot dispatch trip in '{currentStatus}' status. Trip must be Draft.");
    }

    public static void ValidateCompleteTransition(string currentStatus, decimal currentOdometer, decimal finalOdometer, decimal fuelConsumedL)
    {
        if (!string.Equals(currentStatus, TripStatus.Dispatched.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException($"Cannot complete trip in '{currentStatus}' status. Trip must be Dispatched.");

        if (fuelConsumedL <= 0)
            throw new BusinessRuleException("Fuel consumed must be greater than zero liters.");

        if (finalOdometer < currentOdometer)
            throw new BusinessRuleException($"Final odometer reading ({finalOdometer}) cannot be less than vehicle's current odometer ({currentOdometer}).");
    }

    public static void ValidateCancelTransition(string currentStatus)
    {
        if (string.Equals(currentStatus, TripStatus.Completed.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Completed trips cannot be cancelled.");

        if (string.Equals(currentStatus, TripStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Trip is already cancelled.");

        if (!string.Equals(currentStatus, TripStatus.Draft.ToString(), StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(currentStatus, TripStatus.Dispatched.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException($"Invalid trip cancellation from status '{currentStatus}'.");
        }
    }
}

