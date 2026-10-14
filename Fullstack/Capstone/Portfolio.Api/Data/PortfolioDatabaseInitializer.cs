using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Portfolio.Api.Data;

namespace Portfolio.Api.Data;

public static class PortfolioDatabaseInitializer
{
    public static async Task MigrateAsync(PortfolioDbContext database, CancellationToken cancellationToken = default)
    {
        var historyRepository = database.GetService<IHistoryRepository>();
        if (!await historyRepository.ExistsAsync(cancellationToken)
            && await HasLegacyPortfolioSchemaAsync(database, cancellationToken))
        {
            var baselineMigration = database.Database.GetMigrations().FirstOrDefault();
            if (baselineMigration is null)
            {
                throw new InvalidOperationException("The portfolio baseline migration could not be found.");
            }

            await historyRepository.CreateIfNotExistsAsync(cancellationToken);
            var productVersion = typeof(DbContext).Assembly.GetName().Version?.ToString(3) ?? "10.0.13";
            var historyInsert = historyRepository.GetInsertScript(new HistoryRow(baselineMigration, productVersion));
            await database.Database.ExecuteSqlRawAsync(historyInsert, cancellationToken);
        }

        await database.Database.MigrateAsync(cancellationToken);
    }

    private static async Task<bool> HasLegacyPortfolioSchemaAsync(
        PortfolioDbContext database,
        CancellationToken cancellationToken)
    {
        var connection = database.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'PortfolioUsers';";
            return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken)) > 0;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}