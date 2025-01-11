namespace Common.DataProtection.Abstractions;

public interface IDataProtectionService
{
    byte[] Protect(byte[] unprotectedData);
    byte[] Unprotect(byte[] protectedData);
}
