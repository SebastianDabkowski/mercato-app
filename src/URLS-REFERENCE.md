# Mercato Application URLs - Quick Reference

## API (Backend)
**Project**: `SD.Mercato.API`

| Protocol | URL |
|----------|-----|
| HTTP | http://localhost:5201 |
| HTTPS | https://localhost:7075 |
| Swagger UI | https://localhost:7075/swagger |

## UI (Frontend)
**Project**: `SD.Mercato.UI`

| Protocol | URL |
|----------|-----|
| HTTP | http://localhost:5234 |
| HTTPS | https://localhost:7044 |

## Configuration Files

### API Base URL (for UI to call API)
**File**: `src/AppUI/SD.Mercato.UI/SD.Mercato.UI.Client/wwwroot/appsettings.json`
```json
{
  "ApiBaseUrl": "https://localhost:7075/"
}
```

### Database Connection
**File**: `src/API/SD.Mercato.API/appsettings.json`
```
Server=.\\SQLEXPRESS
Database=MercatoDB
User Id=MercatoUser
Password=StrongPassword_123!@#
```

## Startup Order

**IMPORTANT**: Always start the API before the UI!

```bash
# Step 1: Start API (Terminal 1)
cd src/API/SD.Mercato.API
dotnet run
# Wait for: "Now listening on: https://localhost:7075"

# Step 2: Start UI (Terminal 2)
cd src/AppUI/SD.Mercato.UI/SD.Mercato.UI
dotnet run
# Wait for: "Now listening on: https://localhost:7044"

# Step 3: Open browser
# Navigate to: https://localhost:7044
```

## Testing Endpoints

### Health Check
```bash
# API is running
curl https://localhost:7075/swagger
```

### Test API Call
```bash
# Get categories
curl https://localhost:7075/api/categories
```

## Common Issues

| Issue | Solution |
|-------|----------|
| UI can't connect to API | 1. Ensure API is running<br>2. Check `ApiBaseUrl` in client's appsettings.json<br>3. Verify CORS is configured |
| Port already in use | Kill running processes: `Get-Process -Name dotnet \| Stop-Process -Force` |
| CartService not found | 1. Clear browser cache (Ctrl+F5)<br>2. Ensure API is running first<br>3. Rebuild solution |
| Swagger not loading | Check API console for errors, verify it's listening on correct port |

---
**Last Updated**: 2025-01-24
