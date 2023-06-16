using Godot;

namespace GodotSharp.BuildingBlocks
{
    public record Circle(Vector2 Origin, float Radius);

    public static class ShapeExtensions
    {
        public static IEnumerable<Vector2> Slice(this Circle circle, int count, float? start = null)
        {
            if (count == 0) yield break;

            var slice = Const.FullCircle / count;
            start ??= Random.Shared.NextSingle() * Const.FullCircle;

            for (var i = 0; i < count; ++i)
            {
                var angle = start.Value + i * slice;
                var x = circle.Origin.X + circle.Radius * Mathf.Cos(angle);
                var y = circle.Origin.Y + circle.Radius * Mathf.Sin(angle);

                yield return new Vector2(x, y);
            }
        }
    }
}
