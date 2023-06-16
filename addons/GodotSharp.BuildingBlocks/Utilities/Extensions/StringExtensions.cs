using System.Text.RegularExpressions;
using Godot;

namespace GodotSharp.BuildingBlocks
{
    public static partial class StringExtensions
    {
        //[GeneratedRegex(@"^\d+", RegexOptions.Compiled)]
        //private static partial Regex LeadingInts();

        //[GeneratedRegex(@"\d+$", RegexOptions.RightToLeft | RegexOptions.Compiled)]
        //private static partial Regex TrailingInts();

        //public static bool TryGetLeadingInts(this string source, out int value)
        //    => int.TryParse(LeadingInts().Match(source).Value, out value);

        //public static bool TryGetTrailingInts(this string source, out int value)
        //    => int.TryParse(TrailingInts().Match(source).Value, out value);

        public static string Join<T>(this IEnumerable<T> source, string sep = ", ")
            => string.Join(sep, source);

        public static string TrimPrefixN(this string source, string prefix)
            => source.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase) ? source[prefix.Length..] : source;

        public static string TrimSuffixN(this string source, string suffix)
            => source.EndsWith(suffix, StringComparison.CurrentCultureIgnoreCase) ? source[..^suffix.Length] : source;

        public static string StripDigits(this string source, string replace = "")
            => Digits().Replace(source, replace);

        public static string GetFileBaseName(this string source)
        {
            source = source.GetFile();
            var num = source.Find(".");
            return num > 0 ? source[..num] : source;
        }

        public static string GetDirName(this string source)
            => source.GetBaseDir().GetFile();

        public static IEnumerable<string> InNaturalOrder(this ICollection<string> source)
        {
            var digitsRegex = Digits();
            var maxDigits = source.SelectMany(str => digitsRegex.Matches(str).Select(digits => digits.Length)).MaxOrDefault();
            return source.OrderBy(str => digitsRegex.Replace(str, match => match.Value.PadLeft(maxDigits, '0')));
        }

        [GeneratedRegex(@"\d+", RegexOptions.Compiled)]
        private static partial Regex Digits();
    }
}
