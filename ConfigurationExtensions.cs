using Microsoft.Extensions.Configuration;

namespace GraphApi.Configuration
{
    public static class ConfigurationExtensions
    {
        public static string GetClientId(this IConfiguration configuration)
        {
            return configuration["AzureAdB2C:ClientId"] ?? string.Empty;
        }

        public static string GetTenantId(this IConfiguration configuration)
        {
            return configuration["AzureAdB2C:TenantId"] ?? string.Empty;
        }

        public static string GetClientSecret(this IConfiguration configuration)
        {
            return configuration["B2CSettings:ClientSecret"] ?? string.Empty;
        }
    }
}
