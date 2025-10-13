using KomunalkaAPI.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KomunalkaAPI.Services.Background;

/// <summary>
/// Background service для періодичного очищення застарілих refresh токенів
/// </summary>
public class TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger) : BackgroundService
{
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Token Cleanup Service запущено");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTokens();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Token Cleanup Service зупинено");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Помилка при очищенні застарілих токенів");
                // Чекаємо менше часу після помилки
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task CleanupExpiredTokens()
    {
        using var scope = serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            var deletedCount = await unitOfWork.RefreshTokens.DeleteExpiredTokensAsync();

            if (deletedCount > 0)
            {
                await unitOfWork.CompleteAsync();
                logger.LogInformation("Видалено {Count} застарілих refresh токенів", deletedCount);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Помилка при видаленні застарілих токенів з бази даних");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Token Cleanup Service зупиняється...");
        await base.StopAsync(cancellationToken);
    }
}
