using System.Text;
using Common.DataProtection.Abstractions;

namespace Common.DataProtection.Extensions;

public static class DataProtectionServiceExtension
{
    public static string Protect(
        this IDataProtectionService service,
        string plain
    )
    {
        return Convert.ToBase64String(
            service.Protect(Encoding.UTF8.GetBytes(plain))
        );
    }

    public static string Unprotect(
        this IDataProtectionService service,
        string cipher
    )
    {
        return Encoding.UTF8.GetString(
            service.Unprotect(Convert.FromBase64String(cipher))
        );
    }
}
