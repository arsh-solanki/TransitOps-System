using TransitOps.Api.Domain.Rules;
using TransitOps.Api.Exceptions;
using Xunit;

namespace TransitOps.Tests.Domain;

public class DomainRuleTests
{
    [Fact]
    public void VehicleRules_Capacity_Van05_450kg_On_500kg_Allowed()
    {
        // Act & Assert (Should not throw)
        var ex = Record.Exception(() => VehicleRules.ValidateCapacity(450, 500));
        Assert.Null(ex);
    }

    [Fact]
    public void VehicleRules_Capacity_Overweight_Rejected()
    {
        // Act & Assert
        var ex = Assert.Throws<BusinessRuleException>(() => VehicleRules.ValidateCapacity(550, 500));
        Assert.Contains("exceeds vehicle max load capacity", ex.Message);
    }

    [Fact]
    public void DriverRules_License_Expiring_Today_Is_Valid()
    {
        var today = new DateTime(2026, 10, 30);
        var expiry = new DateTime(2026, 10, 30);

        var ex = Record.Exception(() => DriverRules.ValidateLicenseValidity(expiry, today));
        Assert.Null(ex);
    }

    [Fact]
    public void DriverRules_License_Expired_Yesterday_Rejected()
    {
        var today = new DateTime(2026, 10, 30);
        var expiry = new DateTime(2026, 10, 29);

        var ex = Assert.Throws<BusinessRuleException>(() => DriverRules.ValidateLicenseValidity(expiry, today));
        Assert.Contains("expired", ex.Message);
    }

    [Theory]
    [InlineData("Retired")]
    [InlineData("InShop")]
    [InlineData("OnTrip")]
    public void VehicleRules_Assignment_NonAvailable_Statuses_Rejected(string status)
    {
        Assert.Throws<BusinessRuleException>(() => VehicleRules.ValidateForAssignment(status));
    }

    [Fact]
    public void MaintenanceRules_ClosingMaintenance_Keeps_Retired_Vehicles_Retired()
    {
        var nextStatus = MaintenanceRules.DetermineVehicleStatusOnClose("Retired");
        Assert.Equal("Retired", nextStatus);
    }

    [Fact]
    public void MaintenanceRules_ClosingMaintenance_Restores_Active_Vehicles_To_Available()
    {
        var nextStatus = MaintenanceRules.DetermineVehicleStatusOnClose("InShop");
        Assert.Equal("Available", nextStatus);
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public void TripRules_Cancel_InvalidTransitions_Rejected(string status)
    {
        Assert.Throws<BusinessRuleException>(() => TripRules.ValidateCancelTransition(status));
    }

    [Fact]
    public void TripRules_Dispatch_NonDraft_Rejected()
    {
        Assert.Throws<BusinessRuleException>(() => TripRules.ValidateDispatchTransition("Dispatched"));
    }

    [Fact]
    public void TripRules_Complete_LowerFinalOdometer_Rejected()
    {
        Assert.Throws<BusinessRuleException>(() => TripRules.ValidateCompleteTransition("Dispatched", 1000, 950, 50));
    }

    [Fact]
    public void TripRules_Complete_ZeroFuel_Rejected()
    {
        Assert.Throws<BusinessRuleException>(() => TripRules.ValidateCompleteTransition("Dispatched", 1000, 1100, 0));
    }
}

