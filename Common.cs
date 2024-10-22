namespace Get;

internal static class Common
{
    internal static bool IsUpper(this string s)
    {
        foreach (char c in s)
            if (!char.IsUpper(c)) return false;
        return true;
    }

    internal static bool TryOutDouble(this string s, out double result)
    {
        try
        {
            double i = Convert.ToDouble(s);
            result = i;
            return true;
        }
        catch (FormatException) { }
        result = 0;
        return false;
    }

    internal static bool IsDouble(this string s) => s.TryOutDouble(out var _);
}