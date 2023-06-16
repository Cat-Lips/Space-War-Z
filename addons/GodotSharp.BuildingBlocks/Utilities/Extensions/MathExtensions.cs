namespace GodotSharp.BuildingBlocks
{
    public static class MathExtensions
    {
        public static T PickRandom<T>(this IList<T> source)
            => source[Random.Shared.Next(source.Count - 1)];

        public static IEnumerable<T> Randomise<T>(this IEnumerable<T> source)
            => source.OrderBy(_ => Random.Shared.Next());
    }
}
