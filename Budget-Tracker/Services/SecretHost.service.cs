using Abhiram.Secrets.Providers.Interface;
using BudgetTracker.Defination;

namespace BudgetTracker.Services;

public class SecretHostService : IHostedService
{
    private readonly ISecretManager _secretManager;
    private readonly AppSecrets _appSecrets;

    public SecretHostService(ISecretManager secretManager, AppSecrets appSecrets)
    {
        _appSecrets = appSecrets;
        _secretManager = secretManager;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _appSecrets.DataBase = await _secretManager.GetSecretAsync("DB");
        _appSecrets.Host = await _secretManager.GetSecretAsync("HOST");
        _appSecrets.PassWord = await _secretManager.GetSecretAsync("PASSWORD");
        _appSecrets.UserName = await _secretManager.GetSecretAsync("USERNAME");
        _appSecrets.ApiKey = await _secretManager.GetSecretAsync("API_KEY");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}