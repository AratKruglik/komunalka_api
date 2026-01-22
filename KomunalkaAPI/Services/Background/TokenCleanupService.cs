using KomunalkaAPI.Repositories;

namespace KomunalkaAPI.Services.Background;

public class TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger) : BackgroundService
{
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Token Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTokens();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Token Cleanup Service stopped");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error cleaning up expired tokens");
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
                logger.LogInformation("Deleted {Count} expired refresh tokens", deletedCount);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting expired tokens from database");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Token Cleanup Service stopping...");
        await base.StopAsync(cancellationToken);
    }
}
