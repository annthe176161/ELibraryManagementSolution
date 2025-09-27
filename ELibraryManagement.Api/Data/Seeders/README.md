# Seeders Documentation

This folder contains all database seeding functionality for the ELibrary Management API.

## Structure

### `InitialDataSeeder.cs`
- **Purpose**: Contains static data seeding for ModelBuilder (Entity Framework)
- **Usage**: Called automatically during database creation/migration
- **Contains**: 
  - Identity Roles (Admin, Librarian, User)
  - Categories with Vietnamese names
  - Sample books data
- **Method**: `Initialize(ModelBuilder modelBuilder)`

### `AdminUserSeeder.cs`
- **Purpose**: Seeds admin user account and ensures roles exist
- **Usage**: Called manually from Program.cs during application startup
- **Contains**: Admin user creation with proper role assignment
- **Method**: `SeedAdminUser(IServiceProvider serviceProvider)`

### `BookSeeder.cs` 
- **Purpose**: Template/example for additional book seeding
- **Usage**: Currently contains example code (commented)
- **Contains**: Example structure for seeding additional books
- **Method**: `SeedBooks(ModelBuilder modelBuilder)`

### `UserStatusSeeder.cs`
- **Purpose**: Updates existing user status data
- **Usage**: Called manually from Program.cs during application startup  
- **Contains**: SQL command to update user IsActive status
- **Method**: `SeedUserStatusAsync(ApplicationDbContext context)`

## Usage Pattern

1. **Static Data** (Categories, Roles, Sample Books) → Use `InitialDataSeeder` with ModelBuilder
2. **Runtime Data** (Admin User, User Status Updates) → Use specific seeders called from Program.cs
3. **Future Extensions** → Create new seeder classes following the same pattern

## Namespace Convention
All seeders use the namespace: `ELibraryManagement.Api.Data.Seeders`