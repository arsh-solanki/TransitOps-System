using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransitOps.Api.DTOs;
using TransitOps.Api.Services.Interfaces;

namespace TransitOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    [Authorize(Roles = "FleetManager,FinancialAnalyst,SafetyOfficer,Driver")]
    public async Task<ActionResult<List<VehicleResponse>>> GetAll(
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] string? region,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir)
    {
        var vehicles = await _vehicleService.GetAllAsync(type, status, region, search, sortBy, sortDir);
        return Ok(vehicles);
    }

    [HttpGet("available")]
    [Authorize(Roles = "FleetManager,Driver,SafetyOfficer,FinancialAnalyst")]
    public async Task<ActionResult<List<VehicleResponse>>> GetAvailable()
    {
        var vehicles = await _vehicleService.GetAvailableAsync();
        return Ok(vehicles);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "FleetManager,FinancialAnalyst,SafetyOfficer,Driver")]
    public async Task<ActionResult<VehicleResponse>> GetById(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        return Ok(vehicle);
    }

    [HttpPost]
    [Authorize(Roles = "FleetManager")]
    public async Task<ActionResult<VehicleResponse>> Create([FromBody] VehicleCreateRequest request)
    {
        var vehicle = await _vehicleService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "FleetManager")]
    public async Task<IActionResult> Update(int id, [FromBody] VehicleUpdateRequest request)
    {
        await _vehicleService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "FleetManager")]
    public async Task<IActionResult> Delete(int id)
    {
        await _vehicleService.DeleteAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriversController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    [HttpGet]
    [Authorize(Roles = "SafetyOfficer,FleetManager,FinancialAnalyst,Driver")]
    public async Task<ActionResult<List<DriverResponse>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir)
    {
        var drivers = await _driverService.GetAllAsync(status, search, sortBy, sortDir);
        return Ok(drivers);
    }

    [HttpGet("available")]
    [Authorize(Roles = "SafetyOfficer,FleetManager,Driver,FinancialAnalyst")]
    public async Task<ActionResult<List<DriverResponse>>> GetAvailable()
    {
        var drivers = await _driverService.GetAvailableAsync();
        return Ok(drivers);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "SafetyOfficer,FleetManager,FinancialAnalyst,Driver")]
    public async Task<ActionResult<DriverResponse>> GetById(int id)
    {
        var driver = await _driverService.GetByIdAsync(id);
        return Ok(driver);
    }

    [HttpPost]
    [Authorize(Roles = "SafetyOfficer")]
    public async Task<ActionResult<DriverResponse>> Create([FromBody] DriverCreateRequest request)
    {
        var driver = await _driverService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = driver.Id }, driver);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "SafetyOfficer")]
    public async Task<IActionResult> Update(int id, [FromBody] DriverUpdateRequest request)
    {
        await _driverService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "SafetyOfficer")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] DriverStatusUpdateRequest request)
    {
        await _driverService.UpdateStatusAsync(id, request.Status);
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpGet]
    [Authorize(Roles = "FleetManager,Driver,SafetyOfficer,FinancialAnalyst")]
    public async Task<ActionResult<List<TripResponse>>> GetAll([FromQuery] string? status, [FromQuery] int? vehicleId, [FromQuery] int? driverId)
    {
        var trips = await _tripService.GetAllAsync(status, vehicleId, driverId);
        return Ok(trips);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "FleetManager,Driver,SafetyOfficer,FinancialAnalyst")]
    public async Task<ActionResult<TripResponse>> GetById(int id)
    {
        var trip = await _tripService.GetByIdAsync(id);
        return Ok(trip);
    }

    [HttpPost]
    [Authorize(Roles = "FleetManager,Driver")]
    public async Task<ActionResult<TripResponse>> Create([FromBody] TripCreateRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "1";
        int userId = int.TryParse(userIdStr, out var parsed) ? parsed : 1;

        var trip = await _tripService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = trip.Id }, trip);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "FleetManager,Driver")]
    public async Task<IActionResult> Update(int id, [FromBody] TripUpdateRequest request)
    {
        await _tripService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpPost("{id:int}/dispatch")]
    [Authorize(Roles = "FleetManager,Driver")]
    public async Task<IActionResult> Dispatch(int id)
    {
        await _tripService.DispatchAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "FleetManager,Driver")]
    public async Task<IActionResult> Complete(int id, [FromBody] TripCompleteRequest request)
    {
        await _tripService.CompleteAsync(id, request);
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "FleetManager,Driver")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _tripService.CancelAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceController(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    [HttpGet]
    [Authorize(Roles = "FleetManager,FinancialAnalyst")]
    public async Task<ActionResult<List<MaintenanceResponse>>> GetAll()
    {
        var logs = await _maintenanceService.GetAllAsync();
        return Ok(logs);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "FleetManager,FinancialAnalyst")]
    public async Task<ActionResult<MaintenanceResponse>> GetById(int id)
    {
        var log = await _maintenanceService.GetByIdAsync(id);
        return Ok(log);
    }

    [HttpPost]
    [Authorize(Roles = "FleetManager")]
    public async Task<ActionResult<MaintenanceResponse>> Create([FromBody] MaintenanceCreateRequest request)
    {
        var log = await _maintenanceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = log.Id }, log);
    }

    [HttpPost("{id:int}/close")]
    [Authorize(Roles = "FleetManager")]
    public async Task<IActionResult> Close(int id)
    {
        await _maintenanceService.CloseAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/fuel-logs")]
[Authorize]
public class FuelLogsController : ControllerBase
{
    private readonly IFuelLogService _fuelLogService;

    public FuelLogsController(IFuelLogService fuelLogService)
    {
        _fuelLogService = fuelLogService;
    }

    [HttpGet]
    [Authorize(Roles = "FinancialAnalyst,FleetManager,Driver")]
    public async Task<ActionResult<List<FuelLogResponse>>> GetAll(
        [FromQuery] int? vehicleId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var logs = await _fuelLogService.GetAllAsync(vehicleId, startDate, endDate);
        return Ok(logs);
    }

    [HttpPost]
    [Authorize(Roles = "Driver,FleetManager,FinancialAnalyst")]
    public async Task<ActionResult<FuelLogResponse>> Create([FromBody] FuelLogCreateRequest request)
    {
        var log = await _fuelLogService.CreateAsync(request);
        return Created("", log);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpGet]
    [Authorize(Roles = "FinancialAnalyst,FleetManager,Driver")]
    public async Task<ActionResult<List<ExpenseResponse>>> GetAll()
    {
        var expenses = await _expenseService.GetAllAsync();
        return Ok(expenses);
    }

    [HttpPost]
    [Authorize(Roles = "FinancialAnalyst,FleetManager,Driver")]
    public async Task<ActionResult<ExpenseResponse>> Create([FromBody] ExpenseCreateRequest request)
    {
        var expense = await _expenseService.CreateAsync(request);
        return Created("", expense);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get(
        [FromQuery] string? vehicleType,
        [FromQuery] string? status,
        [FromQuery] string? region)
    {
        var dashboard = await _dashboardService.GetDashboardKpisAsync(vehicleType, status, region);
        return Ok(dashboard);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("fuel-efficiency")]
    [Authorize(Roles = "FinancialAnalyst,FleetManager")]
    public async Task<IActionResult> GetFuelEfficiency([FromQuery] string? format)
    {
        var data = await _reportService.GetFuelEfficiencyAsync();
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = _reportService.GenerateCsv(data);
            return File(bytes, "text/csv", "fuel_efficiency_report.csv");
        }
        return Ok(data);
    }

    [HttpGet("utilization")]
    [Authorize(Roles = "FinancialAnalyst,FleetManager")]
    public async Task<IActionResult> GetUtilization([FromQuery] string? format)
    {
        var data = await _reportService.GetUtilizationAsync();
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = _reportService.GenerateCsv(data);
            return File(bytes, "text/csv", "fleet_utilization_report.csv");
        }
        return Ok(data);
    }

    [HttpGet("operational-cost")]
    [Authorize(Roles = "FinancialAnalyst,FleetManager")]
    public async Task<IActionResult> GetOperationalCost([FromQuery] string? format)
    {
        var data = await _reportService.GetOperationalCostAsync();
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = _reportService.GenerateCsv(data);
            return File(bytes, "text/csv", "operational_cost_report.csv");
        }
        return Ok(data);
    }

    [HttpGet("roi")]
    [Authorize(Roles = "FinancialAnalyst,FleetManager")]
    public async Task<IActionResult> GetRoi([FromQuery] string? format)
    {
        var data = await _reportService.GetRoiAsync();
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = _reportService.GenerateCsv(data);
            return File(bytes, "text/csv", "vehicle_roi_report.csv");
        }
        return Ok(data);
    }

    [HttpGet("vehicle-analytics/{vehicleId:int}")]
    [Authorize(Roles = "FinancialAnalyst,FleetManager")]
    public async Task<ActionResult<VehicleAnalyticsResponse>> GetVehicleAnalytics(int vehicleId)
    {
        var data = await _reportService.GetVehicleAnalyticsAsync(vehicleId);
        return Ok(data);
    }
}

