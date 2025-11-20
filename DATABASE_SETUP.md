# Database Setup Instructions

## Running Migrations

Since this is running in a sandboxed environment without SQL Server LocalDB, you'll need to apply the migrations in your local development environment.

### Prerequisites
- SQL Server (LocalDB, Express, or full version)
- .NET 9.0 SDK
- EF Core Tools (`dotnet tool install --global dotnet-ef`)

### Steps to Apply Migrations

1. Update the connection string in `src/API/SD.Mercato.API/appsettings.json` to match your SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MercatoDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

2. Navigate to the Users module directory:
   ```bash
   cd src/Modules/SD.Mercato.Users
   ```

3. Apply the migrations:
   ```bash
   dotnet ef database update --startup-project ../../API/SD.Mercato.API --context UsersDbContext
   ```

This will create the database schema with the following tables:
- `users.AspNetUsers` - User accounts
- `users.AspNetRoles` - Roles (Buyer, Seller, Administrator)
- `users.AspNetUserRoles` - User-Role mapping
- `users.AspNetUserClaims` - User claims
- `users.AspNetUserLogins` - External login providers
- `users.AspNetUserTokens` - Refresh tokens
- `users.AspNetRoleClaims` - Role claims
- `users.SellerStaff` - Seller staff members (future-proof for multi-user stores)

### Running the Application

1. Start the API:
   ```bash
   cd src/API/SD.Mercato.API
   dotnet run
   ```

2. Start the UI (in a separate terminal):
   ```bash
   cd src/AppUI/SD.Mercato.UI/SD.Mercato.UI
   dotnet run
   ```

3. Access the application at `https://localhost:XXXX` (port will be shown in console)

### Testing Authentication

1. Navigate to `/register` to create a new account
2. Choose role: Buyer or Seller
3. After registration, you'll be automatically logged in
4. Access `/profile` to view your profile
5. The navigation will show role-specific menu items based on your role

### OAuth Configuration

To enable Google and Facebook login, you need to:

1. **Google OAuth**:
   - Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Create a new project or select existing
   - Enable Google+ API
   - Create OAuth 2.0 credentials
   - Update `appsettings.json` with ClientId and ClientSecret

2. **Facebook OAuth**:
   - Go to [Facebook Developers](https://developers.facebook.com/)
   - Create a new app
   - Add Facebook Login product
   - Update `appsettings.json` with AppId and AppSecret

### Security Notes

- The JWT SecretKey in `appsettings.json` is for development only. **Change it in production!**
- OAuth client secrets should be stored in User Secrets or Azure Key Vault in production
- Never commit actual OAuth credentials to source control
