using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace KomunalkaAPI.Extensions;

/// <summary>
/// Extension methods for ModelBuilder to configure Laravel-compatible naming conventions
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures all entity names and properties to use snake_case (Laravel convention)
    /// </summary>
    public static void UseLaravelNamingConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Convert table names to snake_case plural
            var tableName = ToSnakeCase(entity.GetTableName());
            entity.SetTableName(tableName);

            // Convert column names to snake_case
            foreach (var property in entity.GetProperties())
            {
                var columnName = ToSnakeCase(property.Name);
                property.SetColumnName(columnName);
            }

            // Convert foreign key names to snake_case
            foreach (var key in entity.GetKeys())
            {
                var keyName = ToSnakeCase(key.GetName());
                key.SetName(keyName);
            }

            // Convert foreign keys to snake_case
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var foreignKeyName = ToSnakeCase(foreignKey.GetConstraintName());
                foreignKey.SetConstraintName(foreignKeyName);
            }

            // Convert indexes to snake_case
            foreach (var index in entity.GetIndexes())
            {
                var indexName = ToSnakeCase(index.GetDatabaseName());
                index.SetDatabaseName(indexName);
            }
        }
    }

    /// <summary>
    /// Converts PascalCase string to snake_case
    /// Examples: UserId -> user_id, IPAddress -> ip_address
    /// </summary>
    private static string ToSnakeCase(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        var startUnderscores = Regex.Match(input, @"^_+");
        return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }
}
