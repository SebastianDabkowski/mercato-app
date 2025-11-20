# Quick Verification Guide

This guide helps you quickly verify that the authentication system is working correctly.

## Prerequisites

- .NET 9.0 SDK installed
- SQL Server (LocalDB, Express, or full version)
- Git repository cloned

## Quick Start (5 minutes)

### 1. Build the Solution

```bash
cd /home/runner/work/mercato-app/mercato-app
dotnet build src/SD.Mercato.sln
```

Expected: `Build succeeded. 0 Warning(s), 0 Error(s)`

### 2. Apply Database Migrations

```bash
cd src/Modules/SD.Mercato.Users
dotnet ef database update --startup-project ../../API/SD.Mercato.API --context UsersDbContext
```

Expected: Database created with all Identity tables

### 3. Start the API

```bash
cd src/API/SD.Mercato.API
dotnet run
```

Expected: 
```
Now listening on: https://localhost:7147
Application started. Press Ctrl+C to shut down.
```

### 4. Test API Endpoints

In a new terminal:

```bash
# Test Registration
curl -X POST https://localhost:7147/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "buyer@test.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "Buyer",
    "role": "Buyer"
  }' \
  -k

# Expected Response (200 OK):
# {
#   "success": true,
#   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "message": "Registration successful",
#   "user": {
#     "id": "...",
#     "email": "buyer@test.com",
#     "firstName": "Test",
#     "lastName": "Buyer",
#     "role": "Buyer",
#     "isEmailVerified": true
#   }
# }
```

Save the token from the response for the next steps.

```bash
# Test Login
curl -X POST https://localhost:7147/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "buyer@test.com",
    "password": "Test123!"
  }' \
  -k

# Expected Response (200 OK):
# Same structure as registration response with a new token
```

```bash
# Test Get Profile (replace YOUR_TOKEN with the actual token)
curl -X GET https://localhost:7147/api/users/profile \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -k

# Expected Response (200 OK):
# {
#   "id": "...",
#   "email": "buyer@test.com",
#   "firstName": "Test",
#   "lastName": "Buyer",
#   "phoneNumber": null,
#   "role": "Buyer",
#   "isEmailVerified": true,
#   "createdAt": "2025-11-20T...",
#   "lastLoginAt": "2025-11-20T..."
# }
```

```bash
# Test Unauthorized Access (without token)
curl -X GET https://localhost:7147/api/users/profile -k

# Expected Response (401 Unauthorized)
```

### 5. Start the UI (Optional)

In a new terminal:

```bash
cd src/AppUI/SD.Mercato.UI/SD.Mercato.UI
dotnet run
```

Expected:
```
Now listening on: https://localhost:XXXX
```

Open your browser to the URL shown.

### 6. Test UI Flows

1. **Navigate to /register**
   - Fill in the registration form
   - Select "Buyer" or "Seller" role
   - Submit
   - Should redirect to home page
   - Navigation should show your name with a dropdown

2. **Navigate to /login**
   - Enter the email and password you just registered
   - Submit
   - Should redirect to home page

3. **Navigate to /profile**
   - Should see your profile information
   - Click "Edit Profile"
   - Change your name
   - Save
   - Should see success message

4. **Check Navigation**
   - If Buyer: Should see "My Profile" in dropdown
   - If Seller: Should see "My Profile", "Seller Dashboard", "My Products", "My Orders"
   - If Administrator: Should see admin-specific menu items

5. **Test Logout**
   - Click "Logout" in dropdown
   - Should redirect to login page
   - Accessing /profile should redirect to login

## Verification Checklist

- [ ] Solution builds without errors
- [ ] Database migrations applied successfully
- [ ] API starts and listens on HTTPS
- [ ] Registration endpoint works (returns token)
- [ ] Login endpoint works (returns token)
- [ ] Profile endpoint works with valid token
- [ ] Profile endpoint returns 401 without token
- [ ] UI loads successfully
- [ ] Can register via UI
- [ ] Can login via UI
- [ ] Can view profile via UI
- [ ] Can edit profile via UI
- [ ] Can logout via UI
- [ ] Navigation shows role-based menu items
- [ ] Accessing /profile when not logged in redirects to /login

## Common Issues

### Issue: Database migration fails with "LocalDB is not supported"

**Solution**: Update connection string in `src/API/SD.Mercato.API/appsettings.json` to point to your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=MercatoDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### Issue: API returns SSL certificate errors

**Solution**: Use `-k` flag with curl to ignore SSL certificate validation in development, or trust the development certificate:

```bash
dotnet dev-certs https --trust
```

### Issue: CORS errors when accessing API from UI

**Solution**: The API is configured to allow all origins in development. If you still see CORS errors, check that the API URL in `src/AppUI/SD.Mercato.UI/SD.Mercato.UI.Client/wwwroot/appsettings.json` matches the actual API URL.

### Issue: Token not being sent with requests

**Solution**: Check browser DevTools → Application → Local Storage. You should see `authToken` and `currentUser` entries. If not, login again.

## Test Users Created

After running the verification, you'll have:

- **buyer@test.com** / Test123! - Buyer role
- Any additional users you created

## Database Tables to Check

Connect to your SQL Server and verify these tables exist in the `users` schema:

- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetUserLogins
- AspNetUserTokens
- AspNetRoleClaims
- SellerStaff

Check that roles are seeded:
```sql
SELECT * FROM users.AspNetRoles;
-- Should show: Buyer, Seller, Administrator
```

## Success Criteria

✅ All checklist items completed
✅ No errors in API console
✅ No errors in browser console
✅ Tokens are generated and validated correctly
✅ Role-based UI works as expected
✅ Database has all required tables
✅ Roles are seeded

## Next Steps

Once verification is complete:

1. Review the code in `src/Modules/SD.Mercato.Users/`
2. Check the API documentation in `API_DOCUMENTATION.md`
3. Review security considerations in `IMPLEMENTATION_SUMMARY.md`
4. Consider implementing:
   - Email verification with actual emails
   - Password reset flow
   - Account lockout after failed attempts
   - Refresh tokens
   - Unit and integration tests

## Support

If you encounter issues:

1. Check the logs in the API console
2. Check browser DevTools console
3. Verify all prerequisites are installed
4. Ensure SQL Server is running
5. Check that ports 7147 (API) and others (UI) are not in use
6. Review the TODO comments in the code for known limitations
