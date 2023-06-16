using Godot;
using NayukiCircle;
using SmallestCircle.Calculation;
using SmallestCircle.Data.Input.Predefined;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    public static class ShapeExtensions
    {
        // https://github.com/kalin-marinov/SmallestCircle
        public static void GetKalinMarinovCircle(this Vector2[] gdPoints, out Vector2 center, out float radius)
        {
            var calcPoints = gdPoints.Select(p => new SmallestCircle.Data.Point(p.X, p.Y)).ToList();
            var calculator = new Calculator(new ListPointsIterator(calcPoints));
            var circle = calculator.CalculateCircle();
            center = new((float)circle.Center.X, (float)circle.Center.Y);
            radius = (float)circle.Radius;
        }

        // https://www.nayuki.io/page/smallest-enclosing-circle
        public static void GetNayukiCircle(this Vector2[] gdPoints, out Vector2 center, out float radius)
        {
            var calcPoints = gdPoints.Select(p => new Point(p.X, p.Y)).ToList();
            var circle = SmallestEnclosingCircle.MakeCircle(calcPoints);
            center = new((float)circle.c.x, (float)circle.c.y);
            radius = (float)circle.r;
        }

        public static void GetRect(this Vector2[] points, out Vector2 pos, out Vector2 size, out Vector2 center)
        {
            var xMin = points.Min(x => x.X);
            var yMin = points.Min(x => x.Y);
            var xMax = points.Max(x => x.X);
            var yMax = points.Max(x => x.Y);
            pos = new(xMin, yMin);
            size = new(xMax - xMin, yMax - yMin);
            center = pos + size * .5f;
        }
    }
}
