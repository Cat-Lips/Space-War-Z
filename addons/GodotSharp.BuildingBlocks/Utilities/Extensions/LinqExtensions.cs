namespace GodotSharp.BuildingBlocks
{
    public static class LinqExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            var i = -1;
            foreach (var item in source)
                action(item, ++i);
        }

        public static T MaxOrDefault<T>(this IEnumerable<T> source, T defaultValue = default)
            => source.DefaultIfEmpty(defaultValue).Max();

        public static TResult MaxOrDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, T defaultValue = default)
            => source.DefaultIfEmpty(defaultValue).Max(selector);
    }
}
