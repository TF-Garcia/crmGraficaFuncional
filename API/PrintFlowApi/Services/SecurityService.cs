using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PrintFlowApi.Data;

namespace PrintFlowApi.Services;

public class SecurityService(PrintFlowDbContext db)
{
    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    public static string CreateResetToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');
    }

    public async Task<bool> ValidateAdminActionPasswordAsync(string? password, CancellationToken cancellationToken)
    {
        var settings = await db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (settings is null || !settings.RequireAdminPasswordForSensitiveActions)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(settings.AdminActionPasswordHash) &&
               !string.IsNullOrWhiteSpace(password) &&
               BCrypt.Net.BCrypt.Verify(password, settings.AdminActionPasswordHash);
    }
}
