using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    public static class ImageExtensions
    {
        public static Error LoadFromBuffer(this Image image, byte[] buffer, string ext) => ext switch
        {
            "png" => image.LoadPngFromBuffer(buffer),
            "jpg" => image.LoadJpgFromBuffer(buffer),
            "webp" => image.LoadWebpFromBuffer(buffer),
            "tga" => image.LoadTgaFromBuffer(buffer),
            "bmp" => image.LoadBmpFromBuffer(buffer),
            _ => throw new NotImplementedException(),
        };

        public static Error Save(this Image image, string path) => path.GetExtension() switch
        {
            "png" => image.SavePng(path),
            "jpg" => image.SaveJpg(path),
            "webp" => image.SaveWebp(path),
            "exr" => image.SaveExr(path),
            _ => throw new NotImplementedException(),
        };

        public static Error Save(this Image image, ref string path, string @default = "png") => path.GetExtension() switch
        {
            "png" => image.SavePng(path),
            "jpg" => image.SaveJpg(path),
            "webp" => image.SaveWebp(path),
            "exr" => image.SaveExr(path),
            _ => image.Save(path = $"{path}.{@default}"),
        };

        public static Image Crop(this Image source) => source.Crop(out var _);
        public static Image Crop(this Image source, in Rect2I bb) => source.Crop(bb, out var _);
        public static Image Crop(this Image source, out Vector2 offset) => source.Crop(source.GetUsedRect(), out offset);
        public static Image Crop(this Image source, in Rect2I bb, out Vector2 offset)
        {
            var image = Image.Create(bb.Size.X, bb.Size.Y, source.HasMipmaps(), source.GetFormat());
            image.BlitRect(source, bb, Vector2I.Zero);
            offset = bb.Position;
            return image;
        }

        public static Image Optimise(this Image source, out Vector2 spritePos, out Vector2 shapeOffset, bool crop = true, bool center = true) => source.Optimise(source.GetUsedRect(), out spritePos, out shapeOffset, crop, center);
        public static Image Optimise(this Image source, Rect2I bb, out Vector2 spritePos, out Vector2 shapeOffset, bool crop = true, bool center = true)
        {
            return crop
                ? CropImage(out spritePos, out shapeOffset)
                : OriginalImage(out spritePos, out shapeOffset);

            Image CropImage(out Vector2 spritePos, out Vector2 shapePos)
            {
                var image = source.Crop(bb, out var offset);
                spritePos = center ? offset - (HalfFull() - HalfCrop()) : offset;
                shapePos = center ? offset - HalfFull() : offset;
                return image;

                Vector2 HalfCrop()
                    => (Vector2)image.GetSize() * .5f;

                Vector2 HalfFull()
                    => (Vector2)source.GetSize() * .5f;
            }

            Image OriginalImage(out Vector2 spritePos, out Vector2 shapePos)
            {
                spritePos = Vector2.Zero;
                shapePos = center ? -HalfImage() : Vector2.Zero;
                return source;

                Vector2 HalfImage()
                    => (Vector2)source.GetSize() * .5f;
            }
        }

        public static ICollection<Vector2[]> ExtractPolygons(this Image source, int? minPoints = null, float? pointRatio = null) => source.ExtractPolygons(0, 0, out var _, minPoints, pointRatio);
        public static ICollection<Vector2[]> ExtractPolygons(this Image source, out float mass, int? minPoints = null, float? pointRatio = null) => source.ExtractPolygons(0, 0, out mass, minPoints, pointRatio);
        public static ICollection<Vector2[]> ExtractPolygons(this Image source, float alphaThreshold, float vertexEpsilon, out float mass, int? minPoints = null, float? pointRatio = null)
        {
            var bm = new Bitmap();
            bm.CreateFromImageAlpha(source, alphaThreshold); // Godot default: 0.1
            mass = bm.GetTrueBitCount();
            var polygons = bm.OpaqueToPolygons(ImageRect(), vertexEpsilon); // Godot default: 2
            return minPoints is not null ? TrimPolygons(minPoints.Value)
                : pointRatio is not null ? TrimPolygons((int)(polygons.Max(x => x.Length) * pointRatio.Value))
                : polygons;

            Rect2I ImageRect()
                => new(Vector2I.Zero, source.GetSize());

            ICollection<Vector2[]> TrimPolygons(int minPoints)
                => polygons.Where(x => x.Length >= minPoints).ToArray();
        }

        public static IEnumerable<Vector2[]> TrimPolygons(this ICollection<Vector2[]> polygons, int? min = null, float trim = 2)
        {
            min ??= (int)(polygons.Max(x => x.Length) / trim);
            return polygons.Where(x => x.Length >= min);
        }

        public static Sprite2D CreateSprite(this Texture2D texture, string name, in Vector2 pos = default, bool centered = true, bool visible = true, bool uniqueName = true) => new()
        {
            Name = name,
            Position = pos,
            Visible = visible,
            Centered = centered,
            UniqueNameInOwner = uniqueName,
            Texture = texture,
        };

        public static IEnumerable<Node2D> CreateShapes(this ICollection<Vector2[]> polygons, string name, Vector2 offset = default, bool active = true, ShapeType type = ShapeType.Polygon)
        {
            if (polygons.Count is > 1) name += "1";
            return polygons.OrderBy(x => x.Length).SelectMany(CreateShapes);

            IEnumerable<Node2D> CreateShapes(Vector2[] points)
            {
                if (type is ShapeType.All)
                {
                    yield return CreateShape(name.Replace("Shape", "Rect"), ShapeType.Rect, points);
                    yield return CreateShape(name.Replace("Shape", "Circle"), ShapeType.Circle, points);
                    yield return CreateShape(name.Replace("Shape", "Polygon"), ShapeType.Polygon, points);
                    yield break;
                }

                yield return CreateShape(name, type, points);

                Node2D CreateShape(string name, ShapeType type, Vector2[] points)
                {
                    return type switch
                    {
                        ShapeType.Rect => CreateRect(),
                        ShapeType.Circle => CreateCircle(),
                        ShapeType.Polygon => CreatePolygon(),
                        _ => throw new NotImplementedException(),
                    };

                    CollisionShape2D CreateRect()
                    {
                        points.GetRect(out var _, out var size, out var center);
                        var shape = new RectangleShape2D { Size = size };
                        return new()
                        {
                            Name = name,
                            Position = center + offset,
                            Visible = active,
                            Disabled = !active,
                            Shape = shape,
                        };
                    }

                    CollisionShape2D CreateCircle()
                    {
                        //points.GetNayukiCircle(out var center, out var radius);
                        points.GetKalinMarinovCircle(out var center, out var radius);
                        var shape = new CircleShape2D { Radius = radius };
                        return new()
                        {
                            Name = name,
                            Position = center + offset,
                            Visible = active,
                            Disabled = !active,
                            Shape = shape,
                        };
                    }

                    CollisionPolygon2D CreatePolygon() => new()
                    {
                        Name = name,
                        Position = offset,
                        Visible = active,
                        Disabled = !active,
                        Polygon = points,
                    };
                }
            }
        }
    }
}
