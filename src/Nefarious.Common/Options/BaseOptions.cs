namespace Nefarious.Common.Options;

public abstract class BaseOptions<T>
{
    public static string SectionName  => GetSectionName();
    private static string GetSectionName()
    {
        var name = typeof(T).Name;
        return name.Contains("Options") 
            ? name.Replace("Options", string.Empty) 
            : throw new ArgumentException($"Record of name '{name}' does not meet the expected naming convention. (<Name>Options).");
    }
}