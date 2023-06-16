using System.Diagnostics;
using System.Text.RegularExpressions;
using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    public record ZipPartInfo(Image Image, string PartName, bool Visible);
    public record PartInfo(string Texture, string PartName, bool Visible, bool Centered, Vector2 SpritePos, Vector2 ShapePos);

    public static partial class ZipParts
    {
        [GeneratedRegex(@"^L(?<id>\d+),R(?<row>\d+),C(?<column>\d+),(?<name>.+?),(?<visibility>visible|hidden),(?<mode>Normal),(?<opacity>\d+)\.(?<ext>png|bmp)$", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
        private static partial Regex PartsRegex();

        public static ICollection<ZipPartInfo> Unpack(string partsZip)
        {
            return Zip.Read(partsZip, x => GetContent(x).ToArray());

            IEnumerable<ZipPartInfo> GetContent(ZipReader zip)
            {
                var idx = 0;
                foreach (var zipFile in zip.GetFiles().InNaturalOrder())
                {
                    var match = PartsRegex().Match(zipFile);
                    if (!match.Success) continue;

                    var layer = match.Groups["id"].Value.ToInt();
                    var name = match.Groups["name"].Value;
                    var visible = match.Groups["visibility"].Value is "visible";
                    var ext = match.Groups["ext"].Value;

                    Debug.Assert(layer == ++idx);

                    var image = new Image();
                    var err = image.LoadFromBuffer(zip.ReadFile(zipFile), ext);
                    if (err.NotOk(ImageError)) continue;

                    yield return new(image, name, visible);

                    string ImageError() => $"Error importing {zipFile}";
                }
            }
        }

        public static IEnumerable<PartInfo> Extract(
            this IEnumerable<ZipPartInfo> zipParts,
            string target, bool crop, bool center)
        {
            foreach (var part in zipParts)
            {
                var optimisedImagePath = $"{target}/{part.PartName}.png";
                DirAccess.MakeDirRecursiveAbsolute(optimisedImagePath.GetBaseDir()).Throw(MakeDirError);

                part.Image.Optimise(out var spritePos, out var shapePos, crop, center)
                    .SavePng(optimisedImagePath).Throw(SaveError);

                yield return new(optimisedImagePath, part.PartName, part.Visible, center, spritePos, shapePos);

                string SaveError() => $"Error saving {optimisedImagePath}";
                string MakeDirError() => $"Error creating {optimisedImagePath.GetBaseDir()}";
            }
        }
    }
}
