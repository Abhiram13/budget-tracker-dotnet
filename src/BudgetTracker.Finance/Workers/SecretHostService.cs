using Abhiram.Secrets.Providers.Interface;
using BudgetTracker.Core.Domain.ValueObject;

namespace BudgetTracker.Api.Workers;

/// <summary>
/// A hosted service responsible for retrieving secrets at application startup
/// and populating the <see cref="AppSecrets"/> singleton with those values.
/// </summary>
public class SecretHostService : IHostedService
{
    private readonly ISecretManager _secretManager;
    private readonly AppSecrets _appSecrets;

    /// <summary>
    /// A hosted service responsible for retrieving secrets at application startup
    /// and populating the <see cref="AppSecrets"/> singleton with those values. <br />
    /// Initializes a new instance of the <see cref="SecretHostService"/> class.
    /// </summary>
    /// <param name="secretManager">The service used to retrieve secrets asynchronously.</param>
    /// <param name="appSecrets">The singleton instance of <see cref="AppSecrets"/> to populate with secret values.</param>
    public SecretHostService(ISecretManager secretManager, AppSecrets appSecrets)
    {
        _appSecrets = appSecrets;
        _secretManager = secretManager;
    }

    /// <summary>
    /// Called by the host when the application is starting.
    /// Retrieves secret values asynchronously and populates the <see cref="AppSecrets"/> instance.
    /// </summary>
    /// <param name="cancellationToken">A token to signal cancellation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _appSecrets.DataBase = await _secretManager.GetSecretAsync("DB");
        _appSecrets.Host = await _secretManager.GetSecretAsync("HOST");
        _appSecrets.PassWord = await _secretManager.GetSecretAsync("PASSWORD");
        _appSecrets.UserName = await _secretManager.GetSecretAsync("USERNAME");
        _appSecrets.ApiKey = await _secretManager.GetSecretAsync("API_KEY");
    }

    /// <summary>
    /// Called by the host when the application is shutting down.
    /// This implementation does nothing.
    /// </summary>
    /// <param name="cancellationToken">A token to signal cancellation.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}