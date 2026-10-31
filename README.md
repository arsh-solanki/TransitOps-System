# TransitOps Backend

TransitOps is a digital transport operations management platform built with ASP.NET Core Web API, ADO.NET, Microsoft SQL Server, and module-wise stored procedures using parameterized dynamic SQL. It provides role-based asset lifecycle enforcement for vehicles, drivers, trips, maintenance, and expenses, alongside operational analytics.

---

## 1. Prerequisites

- **.NET SDK 10.0** (or compatible .NET runtime)
- **Microsoft SQL Server** (2019+ or Azure SQL / LocalDB)
- **SQL Server Management Studio (SSMS)** or `sqlcmd` / Azure Data Studio

---

## 2. Database Setup & Initialization

Run the database scripts in `TransitOps.Api/Database/` on your SQL Server instance in this exact order:

1. **`01_Tables.sql`**
   - Creates the `TransitOpsDb` database and 8 relational tables: `Roles`, `Users`, `Vehicles`, `Drivers`, `Trips`, `MaintenanceLogs`, `FuelLogs`, `Expenses`.
   - Establishes primary keys, foreign keys, CHECK constraints, and concurrency backstop filtered unique indexes (`UX_Trips_Vehicle_Dispatched`, `UX_Trips_Driver_Dispatched`, `UX_MaintenanceLogs_Vehicle_Active`).
2. **`02_StoredProcedures.sql`**
   - Creates or alters the 9 module-wise stored procedures with `@Mode` routing and parameterized dynamic SQL:
     - `dbo.sp_Auth`
     - `dbo.sp_Vehicle`
     - `dbo.sp_Driver`
     - `dbo.sp_Trip`
     - `dbo.sp_Maintenance`
     - `dbo.sp_FuelLog`
     - `dbo.sp_Expense`
     - `dbo.sp_Dashboard`
     - `dbo.sp_Report`
3. **`03_Seed.sql`**
   - Ensures the 4 core system roles exist (`FleetManager`, `Driver`, `SafetyOfficer`, `FinancialAnalyst`).
   - Default demo users are automatically seeded by `DatabaseSeeder.cs` upon API startup if they do not exist.

---

## 3. Configuration

### Connection String & JWT Secret Key
Configuration is loaded hierarchically from `appsettings.json` and `appsettings.Development.json` (or environment variables).

- **`appsettings.json`**: Contains an empty placeholder for `"Jwt:Key"` for production safety.
- **`appsettings.Development.json`** *(ignored in git)*: Used for local development secrets.

Example configuration in `TransitOps.Api/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=TransitOpsDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  },
  "Jwt": {
    "Key": "TransitOps_SuperSecret_DevelopmentKey_2026_SecureKeyForLocalDev12345!",
    "Issuer": "TransitOps",
    "Audience": "TransitOpsUsers",
    "ExpiryMinutes": "1440"
  }
}
```

> **Security Rule:** `Jwt:Key` must be at least 32 characters long. If missing or shorter than 32 characters, the application throws an `InvalidOperationException` immediately upon startup.

---

## 4. How to Run the API

From the root repository directory:

```bash
# Build the solution
dotnet build TransitOps.sln

# Run unit & domain tests
dotnet test TransitOps.Tests/TransitOps.Tests.csproj

# Launch the Web API
dotnet run --project TransitOps.Api/TransitOps.Api.csproj
```

Once running, access Swagger UI at:
```text
https://localhost:7081/swagger
http://localhost:5242/swagger
```

---

## 5. Demo User Credentials per Role

| Role | Email | Password | Allowed Capabilities |
|------|-------|----------|----------------------|
| **FleetManager** | `manager@transitops.com` | `Test@123` | Fleet registry CRUD, dispatch & trip lifecycle, maintenance scheduling & closure, operational reports |
| **Driver** | `driver@transitops.com` | `Test@123` | View assigned trips, create/dispatch trips, log trip completion & fuel entries |
| **SafetyOfficer** | `safety@transitops.com` | `Test@123` | Driver registry CRUD, license expiry monitoring, status changes (e.g. Suspended) |
| **FinancialAnalyst** | `finance@transitops.com` | `Test@123` | View fleet costs, log operational expenses, fuel & maintenance ROI analytics, CSV report exports |

---

## 6. Official 9-Step "Van-05" Workflow & Endpoint Sequence

Authenticate via `POST /api/auth/login` with `manager@transitops.com` / `Test@123` to obtain a JWT Bearer token, then include `Authorization: Bearer <token>` in subsequent requests:

### Step 1: Register Vehicle "Van-05" (Capacity 500 kg, Status Available)
- **Endpoint:** `POST /api/vehicles`
- **Request Body:**
  ```json
  {
    "registrationNumber": "Van-05",
    "name": "Transit Courier Van",
    "type": "Van",
    "region": "North",
    "maxLoadKg": 500.0,
    "odometer": 12000.0,
    "acquisitionCost": 35000.0,
    "status": "Available"
  }
  ```
- **Response:** `201 Created` with vehicle `id` (e.g. `1`).

### Step 2: Register Driver "Alex" with a Valid License
- **Endpoint:** `POST /api/drivers`
- **Request Body:**
  ```json
  {
    "name": "Alex",
    "licenseNumber": "DL-ALEX-9988",
    "licenseCategory": "Commercial",
    "licenseExpiry": "2027-12-31",
    "contactNumber": "+1-555-0199",
    "safetyScore": 95,
    "status": "Available"
  }
  ```
- **Response:** `201 Created` with driver `id` (e.g. `1`).

### Step 3: Create Trip with Cargo 450 kg (Draft Status)
- **Endpoint:** `POST /api/trips`
- **Request Body:**
  ```json
  {
    "source": "Warehouse Central",
    "destination": "Regional Depot B",
    "vehicleId": 1,
    "driverId": 1,
    "cargoWeightKg": 450.0,
    "plannedDistanceKm": 85.0,
    "revenue": 450.0
  }
  ```
- **Rule Checked:** Cargo weight 450 kg <= Vehicle Max Load 500 kg.
- **Response:** `201 Created` with trip `id` (e.g. `1`), status `Draft`.

### Step 4: Dispatch the Trip
- **Endpoint:** `POST /api/trips/1/dispatch`
- **Rule Checked:** Trip must be `Draft`; Vehicle and Driver must be `Available` and license valid.
- **Response:** `204 NoContent`.

### Step 5: Verify Vehicle & Driver are OnTrip
- **Endpoint:** `GET /api/vehicles/1` -> Response shows `"status": "OnTrip"`.
- **Endpoint:** `GET /api/drivers/1` -> Response shows `"status": "OnTrip"`.
- **Endpoint:** `GET /api/vehicles/available` -> Van-05 is hidden from the dispatch pool.

### Step 6: Complete Trip with Final Odometer and Fuel Consumed
- **Endpoint:** `POST /api/trips/1/complete`
- **Request Body:**
  ```json
  {
    "finalOdometer": 12085.0,
    "fuelConsumedL": 8.5
  }
  ```
- **Rule Checked:** `finalOdometer` (12085) >= current vehicle odometer (12000).
- **Response:** `204 NoContent`. Trip status becomes `Completed`, distance computed as 85 km.

### Step 7: Verify Vehicle & Driver Restored to Available
- **Endpoint:** `GET /api/vehicles/1` -> Response shows `"status": "Available"`, `"odometer": 12085.0`.
- **Endpoint:** `GET /api/drivers/1` -> Response shows `"status": "Available"`.

### Step 8: Create Maintenance (Oil Change)
- **Endpoint:** `POST /api/maintenance`
- **Request Body:**
  ```json
  {
    "vehicleId": 1,
    "description": "Oil Change & Filter Replacement",
    "cost": 150.0
  }
  ```
- **Rule Checked:** Vehicle must not be `OnTrip` or `Retired`. Vehicle status automatically transitions to `InShop`.
- **Response:** `201 Created`. Vehicle is excluded from `GET /api/vehicles/available`.

### Step 9: Reports Reflect Operational Cost & Fuel Efficiency
- **Endpoint:** `GET /api/reports/operational-cost`
  - Total operational cost computes: Maintenance ($150.00) + Fuel.
- **Endpoint:** `GET /api/reports/fuel-efficiency`
  - KmPerLiter computes: 85 km / 8.5 L = 10.00 km/L.
- **Export to CSV:**
  - `GET /api/reports/operational-cost?format=csv` -> returns `operational_cost_report.csv` as `text/csv`.
  - `GET /api/reports/fuel-efficiency?format=csv` -> returns `fuel_efficiency_report.csv` as `text/csv`.

