namespace Common.Configuration.Extensions;

public static class TypeExtension
{
    public static string? FullNameSection(this Type type)
    {
        return type.FullName?.Replace('.', ':');
    }
}
