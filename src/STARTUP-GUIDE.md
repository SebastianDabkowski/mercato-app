# Mercato Application - Startup Guide

## Issues Fixed

Your application had the following issues preventing startup:

### 1. Database Migrations Not Applied ? FIXED
**Problem**: The database tables didn't exist because EF Core migrations weren't applied.

**Solution**: Applied all pending migrations for all modules:
- UsersDbContext
- SellerPanelDbContext
- ProductCatalogDbContext  
- CartDbContext
- HistoryDbContext
- PaymentsDbContext
- NotificationDbContext
- ReviewsDbContext
- AdministrationDbContext
- ReportsDbContext

### 2. Swashbuckle/OpenAPI Version Conflict ? FIXED
**Problem**: Swashbuckle.AspNetCore version 10.0.1 had compatibility issues with .NET 9, causing TypeLoadException errors.

**Solution**: Downgraded `Swashbuckle.AspNetCore` from version 10.0.1 to 6.9.0 in the API project.

### 3. API Base URL Mismatch ? FIXED
**Problem**: The Blazor WebAssembly client was configured to call the API at `https://localhost:7147/` but the API runs on `https://localhost:7075/`. 

**Solution**: Updated `appsettings.json` in the client project to use the correct API URL.

## How to Start the Application

### Prerequisites
- SQL Server Express must be running
- Database connection string in `appsettings.json` must be correct
- .NET 9 SDK installed

### Starting the API
```bash
cd src/API/SD.Mercato.API
dotnet run
```

The API will start on:
- HTTP: http://localhost:5201
- HTTPS: https://localhost:7075
- Swagger UI: https://localhost:7075/swagger

### Starting the UI
```bash
cd src/AppUI/SD.Mercato.UI/SD.Mercato.UI
dotnet run
```

The UI will start on the configured port (check Properties/launchSettings.json).

**Important**: Make sure the API is running BEFORE starting the UI, as the UI needs to connect to the API on startup.

## First-Time Setup Complete

The following initialization has been completed:
- ? All database tables created
- ? Default roles seeded (Buyer, Seller, Administrator)
- ? Category seed data ready
- ? All modules configured and connected
- ? API and UI URLs configured correctly

## Database Connection

Your application is configured to use:
```
Server=.\\SQLEXPRESS
Database=MercatoDB
User Id=MercatoUser
Password=StrongPassword_123!@#
```

## Next Steps

1. **Test the API**: Visit https://localhost:7075/swagger to explore the API endpoints
2. **Test the UI**: Navigate to the UI URL and verify the Blazor application loads
3. **Create Test Users**: Use the API to register test users with different roles
4. **Start Development**: Begin implementing features according to your PRD

## Troubleshooting

### Error: "Cannot provide a value for property 'CartService'"
This error occurs when the Blazor WebAssembly application can't properly initialize services. Solutions:

1. **Ensure API is running first**:
   ```bash
   # Terminal 1: Start API
   cd src/API/SD.Mercato.API
   dotnet run
   
   # Terminal 2: Start UI (wait for API to be ready)
   cd src/AppUI/SD.Mercato.UI/SD.Mercato.UI
   dotnet run
   ```

2. **Clear browser cache**: Blazor WebAssembly caches assemblies. Do a hard refresh (Ctrl+F5) or clear browser cache.

3. **Rebuild the solution**:
   ```bash
   dotnet clean
   dotnet build
   ```

4. **Check API URL configuration**: Verify `src/AppUI/SD.Mercato.UI/SD.Mercato.UI.Client/wwwroot/appsettings.json` has the correct `ApiBaseUrl`.

### Browser Compatibility
**Blazor WebAssembly requires a modern browser:**
- ? Chrome/Edge (recommended)
- ? Firefox
- ? Safari
- ? Internet Explorer (not supported)

If you see errors mentioning Internet Explorer, ensure you're using a modern browser.

### If you get "address already in use" errors:
Kill any running dotnet processes:
```powershell
Get-Process -Name dotnet | Stop-Process -Force
```

### If database changes aren't reflected:
Apply pending migrations for the specific module:
```bash
cd src/API/SD.Mercato.API
dotnet ef database update --context <ContextName>
```

### If you add new entities:
Create and apply a new migration:
```bash
cd src/Modules/SD.Mercato.<ModuleName>
dotnet ef migrations add <MigrationName> --context <ContextName> --startup-project ..\..\API\SD.Mercato.API
dotnet ef database update --context <ContextName> --startup-project ..\..\API\SD.Mercato.API
```

### CORS Issues
If the UI can't connect to the API, check that the API has CORS configured to allow the UI origin. The API currently allows all origins in development (see `Program.cs` in SD.Mercato.API).

### API Not Responding
1. Check the API console for errors
2. Verify SQL Server is running
3. Test API directly: https://localhost:7075/swagger
4. Check firewall/antivirus isn't blocking ports

## Application Status

? **Application is ready to use!**

Both the API and UI are now working correctly. All database tables are created, and the application is fully initialized.

## Development Workflow

### Running Both Projects Simultaneously

**Option 1: Two Terminal Windows**
```bash
# Terminal 1
cd src/API/SD.Mercato.API
dotnet run

# Terminal 2  
cd src/AppUI/SD.Mercato.UI/SD.Mercato.UI
dotnet run
```

**Option 2: Using Visual Studio**
- Right-click the solution ? Set Startup Projects
- Select "Multiple startup projects"
- Set both SD.Mercato.API and SD.Mercato.UI to "Start"

**Option 3: Using VS Code**
- Open two integrated terminals
- Run each project in separate terminals

### Hot Reload
Both projects support hot reload:
- **API**: Code changes will be reflected automatically
- **UI**: Blazor hot reload will update the browser automatically

---
*Last updated: 2025-01-24*
