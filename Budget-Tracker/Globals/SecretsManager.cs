using Google.Cloud.SecretManager.V1;

namespace BudgetTracker.Application;

public class SecretsManager
{
    private readonly string _projectId;
        
    public SecretsManager()
    {
        _projectId = GetProjectId();
    }

    private string GetProjectId()
    {
        string? projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT_ID");
        if (string.IsNullOrEmpty(projectId))
        {
            projectId = ""; // Project-ID
        }
        
        return projectId;
    }

    public async Task<string> GetSecretAsync(string secretId)
    {
        SecretManagerServiceClient client = await SecretManagerServiceClient.CreateAsync();
        SecretVersionName secretVersionName = SecretVersionName.FromProjectSecretSecretVersion(_projectId, secretId, "latest");
        AccessSecretVersionResponse result = await client.AccessSecretVersionAsync(secretVersionName);
        string secretValue = result.Payload.Data.ToStringUtf8();
        return secretValue;
    }
}

public static class Secrets
{
    private static readonly SecretsManager _secretsManager;
    public static readonly string API_KEY;
    public static readonly string HOST;
    public static readonly string DB;
    public static readonly string PASSWORD;
    public static readonly string USERNAME;

    static Secrets()
    {
        _secretsManager = new SecretsManager();
        API_KEY = FetchSecret("API_KEY");
        HOST = FetchSecret("HOST");
        DB = FetchSecret("DB");
        PASSWORD = FetchSecret("PASSWORD");
        USERNAME = FetchSecret("USERNAME");
    }

    static string FetchSecretFromGCP(string secretKey)
    {
        try
        {
            string secretValue = _secretsManager.GetSecretAsync(secretKey).Result;
            return secretValue;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    static string? FetchFromLocal(string secretKey)
    {
        string? secretValue = Environment.GetEnvironmentVariable(secretKey);
        return secretValue;
    }

    static string FetchSecret(string secretKey)
    {
        string? secretValue = FetchSecretFromGCP(secretKey);

        if (string.IsNullOrEmpty(secretValue))
        {
            secretValue = FetchFromLocal(secretKey);
        }

        if (string.IsNullOrEmpty(secretValue))
        {
            throw new KeyNotFoundException($"Secret key \"{secretKey}\" was not found in GCP or local.");
        }

        return secretValue;
    }
}