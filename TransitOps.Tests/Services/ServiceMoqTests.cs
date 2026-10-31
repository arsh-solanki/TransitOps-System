using Moq;
using TransitOps.Api.Base;
using TransitOps.Api.DTOs;
using TransitOps.Api.Exceptions;
using TransitOps.Api.Models;
using TransitOps.Api.Repositories.Interfaces;
using TransitOps.Api.Services;
using Xunit;

namespace TransitOps.Tests.Services;

public class ServiceMoqTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepoMock;
    private readonly Mock<IDriverRepository> _driverRepoMock;
    private readonly Mock<ITripRepository> _tripRepoMock;
    private readonly Mock<IMaintenanceRepository> _maintenanceRepoMock;

    public ServiceMoqTests()
    {
        _vehicleRepoMock = new Mock<IVehicleRepository>();
        _driverRepoMock = new Mock<IDriverRepository>();
        _tripRepoMock = new Mock<ITripRepository>();
        _maintenanceRepoMock = new Mock<IMaintenanceRepository>();
    }

    [Fact]
    public async Task VehicleService_ResultCode_1_Throws_NotFoundException()
    {
        // ResultCode 1 from repository maps to NotFoundException
        var vehicle = new Vehicle { Id = 99, RegistrationNumber = "V-99", Name = "Van", Type = "Van", Region = "North", MaxLoadKg = 1000, AcquisitionCost = 50000 };
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync(vehicle);
        _vehicleRepoMock.Setup(r => r.DeleteAsync(99)).ReturnsAsync(new SpResult { ResultCode = 1, Message = "Vehicle not found." });

        var service = new VehicleService(_vehicleRepoMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(99));
    }

    [Fact]
    public async Task VehicleService_ResultCode_2_Throws_BusinessRuleException()
    {
        // ResultCode 2 represents general business rule violation
        var createRequest = new VehicleCreateRequest
        {
            RegistrationNumber = "VAN-01",
            Name = "Delivery Van",
            Type = "Van",
            Region = "East",
            MaxLoadKg = 1000,
            AcquisitionCost = 25000
        };

        _vehicleRepoMock.Setup(r => r.CreateAsync(It.IsAny<Vehicle>()))
            .ReturnsAsync(new SpResult { ResultCode = 2, Message = "Invalid vehicle configuration." });

        var service = new VehicleService(_vehicleRepoMock.Object);

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(createRequest));
    }

    [Fact]
    public async Task VehicleService_DuplicateRegistration_ResultCode_3_Throws_ConflictException()
    {
        // ResultCode 3 represents conflict/duplicate
        var createRequest = new VehicleCreateRequest
        {
            RegistrationNumber = "VAN-01",
            Name = "Delivery Van",
            Type = "Van",
            Region = "East",
            MaxLoadKg = 1000,
            AcquisitionCost = 25000
        };

        _vehicleRepoMock.Setup(r => r.CreateAsync(It.IsAny<Vehicle>()))
            .ReturnsAsync(new SpResult { ResultCode = 3, Message = "A vehicle with registration number 'VAN-01' already exists." });

        var service = new VehicleService(_vehicleRepoMock.Object);

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(createRequest));
    }

    [Fact]
    public async Task TripService_ResultCode_3_Throws_ConflictException()
    {
        // ResultCode 3 represents concurrency or active conflict
        var trip = new Trip { Id = 10, VehicleId = 1, DriverId = 2, CargoWeightKg = 300, PlannedDistanceKm = 50, Status = "Draft" };
        var vehicle = new Vehicle { Id = 1, Status = "Available", MaxLoadKg = 500 };
        var driver = new Driver { Id = 2, Status = "Available", LicenseExpiry = DateTime.UtcNow.AddYears(1) };

        _tripRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(trip);
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vehicle);
        _driverRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(driver);

        _tripRepoMock.Setup(r => r.DispatchAsync(10))
            .ReturnsAsync(new SpResult { ResultCode = 3, Message = "Conflict: Vehicle or driver is already on another active dispatched trip." });

        var service = new TripService(_tripRepoMock.Object, _vehicleRepoMock.Object, _driverRepoMock.Object);

        await Assert.ThrowsAsync<ConflictException>(() => service.DispatchAsync(10));
    }

    [Fact]
    public async Task MaintenanceService_ActiveConflict_Throws_ConflictException()
    {
        var vehicle = new Vehicle { Id = 5, Status = "InShop" };
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(vehicle);

        var service = new MaintenanceService(_maintenanceRepoMock.Object, _vehicleRepoMock.Object);

        var req = new MaintenanceCreateRequest { VehicleId = 5, Description = "Brake Check", Cost = 300 };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(req));
    }
}

