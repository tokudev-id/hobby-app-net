using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HobbyApp.Infrastructure.Persistence;

public class DatabaseMigrationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(AppDbContext context, ILogger<DatabaseMigrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Starting database migration...");

            // Check if database exists
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                _logger.LogInformation("Database does not exist, it will be created during migration.");
            }
            else
            {
                // Check if this is a fresh database with tables but no migration history
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                
                if (!appliedMigrations.Any() && pendingMigrations.Any())
                {
                    // Check if tables already exist (indicating EnsureCreated was used before)
                    var tablesExist = await CheckIfTablesExistAsync();
                    if (tablesExist)
                    {
                        _logger.LogWarning("Tables exist but no migration history found. This might be due to previous EnsureCreated usage.");
                        _logger.LogInformation("Marking all migrations as applied to fix migration history...");
                        
                        // Mark all migrations as applied without actually running them
                        var migrationIds = pendingMigrations.ToList();
                        foreach (var migrationId in migrationIds)
                        {
                            await _context.Database.ExecuteSqlRawAsync(
                                "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ({0}, {1})",
                                migrationId, "8.0.0");
                        }
                        
                        _logger.LogInformation("Migration history updated successfully.");
                        return;
                    }
                }
            }

            // Apply all pending migrations (this will create the database if it doesn't exist)
            await _context.Database.MigrateAsync();

            _logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }

    private async Task<bool> CheckIfTablesExistAsync()
    {
        try
        {
            // Try to query a table that should exist if the database was created
            await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM Users LIMIT 1");
            return true;
        }
        catch
        {
            return false;
        }
    }


}

