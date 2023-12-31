/* 
 * Smallest enclosing circle - Library (C#)
 * 
 * Copyright (c) 2020 Project Nayuki
 * https://www.nayuki.io/page/smallest-enclosing-circle
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program (see COPYING.txt and COPYING.LESSER.txt).
 * If not, see <http://www.gnu.org/licenses/>.
 */

namespace NayukiCircle
{
    public sealed class SmallestEnclosingCircle
    {

        /* 
         * Returns the smallest circle that encloses all the given points. Runs in expected O(n) time, randomized.
         * Note: If 0 points are given, a circle of radius -1 is returned. If 1 point is given, a circle of radius 0 is returned.
         */
        // Initially: No boundary points known
        public static Circle MakeCircle(IList<Point> points)
        {
            // Clone list to preserve the caller's data, do Durstenfeld shuffle
            var shuffled = new List<Point>(points);
            var rand = new Random();
            for (var i = shuffled.Count - 1; i > 0; i--)
            {
                var j = rand.Next(i + 1);
                (shuffled[j], shuffled[i]) = (shuffled[i], shuffled[j]);
            }

            // Progressively add points to circle or recompute circle
            var c = Circle.INVALID;
            for (var i = 0; i < shuffled.Count; i++)
            {
                var p = shuffled[i];
                if (c.r < 0 || !c.Contains(p))
                    c = MakeCircleOnePoint(shuffled.GetRange(0, i + 1), p);
            }
            return c;
        }

        // One boundary point known
        private static Circle MakeCircleOnePoint(List<Point> points, Point p)
        {
            var c = new Circle(p, 0);
            for (var i = 0; i < points.Count; i++)
            {
                var q = points[i];
                if (!c.Contains(q))
                {
                    c = c.r == 0 ? MakeDiameter(p, q) : MakeCircleTwoPoints(points.GetRange(0, i + 1), p, q);
                }
            }
            return c;
        }

        // Two boundary points known
        private static Circle MakeCircleTwoPoints(List<Point> points, Point p, Point q)
        {
            var circ = MakeDiameter(p, q);
            var left = Circle.INVALID;
            var right = Circle.INVALID;

            // For each point not in the two-point circle
            var pq = q.Subtract(p);
            foreach (var r in points)
            {
                if (circ.Contains(r))
                    continue;

                // Form a circumcircle and classify it on left or right side
                var cross = pq.Cross(r.Subtract(p));
                var c = MakeCircumcircle(p, q, r);
                if (c.r < 0)
                    continue;
                else if (cross > 0 && (left.r < 0 || pq.Cross(c.c.Subtract(p)) > pq.Cross(left.c.Subtract(p))))
                    left = c;
                else if (cross < 0 && (right.r < 0 || pq.Cross(c.c.Subtract(p)) < pq.Cross(right.c.Subtract(p))))
                    right = c;
            }

            // Select which circle to return
            if (left.r < 0 && right.r < 0)
                return circ;
            else return left.r < 0 ? right : right.r < 0 ? left : left.r <= right.r ? left : right;
        }

        public static Circle MakeDiameter(Point a, Point b)
        {
            var c = new Point((a.x + b.x) / 2, (a.y + b.y) / 2);
            return new Circle(c, Math.Max(c.Distance(a), c.Distance(b)));
        }

        public static Circle MakeCircumcircle(Point a, Point b, Point c)
        {
            // Mathematical algorithm from Wikipedia: Circumscribed circle
            var ox = (Math.Min(Math.Min(a.x, b.x), c.x) + Math.Max(Math.Max(a.x, b.x), c.x)) / 2;
            var oy = (Math.Min(Math.Min(a.y, b.y), c.y) + Math.Max(Math.Max(a.y, b.y), c.y)) / 2;
            double ax = a.x - ox, ay = a.y - oy;
            double bx = b.x - ox, by = b.y - oy;
            double cx = c.x - ox, cy = c.y - oy;
            var d = (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by)) * 2;
            if (d == 0)
                return Circle.INVALID;
            var x = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
            var y = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;
            var p = new Point(ox + x, oy + y);
            var r = Math.Max(Math.Max(p.Distance(a), p.Distance(b)), p.Distance(c));
            return new Circle(p, r);
        }

    }

    public struct Circle
    {

        public static readonly Circle INVALID = new(new Point(0, 0), -1);

        private const double MULTIPLICATIVE_EPSILON = 1 + 1e-14;

        public Point c;   // Center
        public double r;  // Radius

        public Circle(Point c, double r)
        {
            this.c = c;
            this.r = r;
        }

        public bool Contains(Point p) => c.Distance(p) <= r * MULTIPLICATIVE_EPSILON;

        public bool Contains(ICollection<Point> ps)
        {
            foreach (var p in ps)
            {
                if (!Contains(p))
                    return false;
            }
            return true;
        }

    }

    public struct Point
    {

        public double x;
        public double y;

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Point Subtract(Point p) => new(x - p.x, y - p.y);

        public double Distance(Point p)
        {
            var dx = x - p.x;
            var dy = y - p.y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Signed area / determinant thing
        public double Cross(Point p) => x * p.y - y * p.x;

    }
}
