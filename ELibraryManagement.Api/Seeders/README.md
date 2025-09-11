# Seeders Folder

This folder contains all database seeding classes for the ELibrary Management API.

## Files

### AdminUserSeeder.cs

- **Purpose**: Seeds the initial admin user and roles into the database
- **Usage**: Automatically called when the application starts
- **Contains**:
  - Admin user creation (admin@elibrary.com / Admin@123)
  - Role creation (Admin, Librarian, User)
  - Database migration execution

### BookSeeder.cs

- **Purpose**: Example seeder for additional books (currently commented out)
- **Usage**: Can be used to seed additional books beyond the main SeedData.cs
- **Status**: Template/example file

## Usage

### Automatic Seeding

The `AdminUserSeeder` is automatically executed when the application starts in `Program.cs`:

```csharp
// Seed admin user
using (var scope = app.Services.CreateScope())
{
    await ELibraryManagement.Api.Seeders.AdminUserSeeder.SeedAdminUser(scope.ServiceProvider);
}
```

### Manual Seeding

You can also call seeders manually in your code:

```csharp
await ELibraryManagement.Api.Seeders.AdminUserSeeder.SeedAdminUser(serviceProvider);
```

## Adding New Seeders

1. Create a new class in this folder
2. Use namespace: `ELibraryManagement.Api.Seeders`
3. Follow the pattern of existing seeders
4. Add the call to `Program.cs` if needed

## Best Practices

- Keep seeding logic separate from business logic
- Use meaningful names for seeder classes
- Include error handling in seeders
- Document what each seeder does
- Test seeders in development environment first
