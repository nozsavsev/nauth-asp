using System.Text.RegularExpressions;

namespace nauth_asp.Helpers
{
    public static class StringExtensions
    {
        public static string SplitPascalCase(this string input)
        {
            return Regex.Replace(input, "([A-Z])", " $1").Trim();
        }
    }
}
