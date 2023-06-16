using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    public record ZipItemInfo(Image Image, string ItemName);
    public record ItemInfo(string Texture, string ItemName, bool Centered, Vector2 SpritePos, Vector2 ShapePos);

    public static class ZipItems
    {
        public static ICollection<ZipItemInfo> Unpack(string itemsZip)
        {
            return Zip.Read(itemsZip, x => GetContent(x).ToArray());

            IEnumerable<ZipItemInfo> GetContent(ZipReader zip)
            {
                foreach (var zipFile in zip.GetFiles())
                {
                    var imageExt = zipFile.GetExtension();
                    if (imageExt is not "png" or "bmp") continue;

                    var image = new Image();
                    var err = image.LoadFromBuffer(zip.ReadFile(zipFile), imageExt);
                    if (err.NotOk(ItemError)) continue;

                    yield return new(image, zipFile.GetFileBaseName());

                    string ItemError() => $"Error importing {zipFile}";
                }
            }
        }

        public static IEnumerable<ItemInfo> Extract(
            this IEnumerable<ZipItemInfo> zipItems,
            string target, bool crop, bool center)
        {
            foreach (var item in zipItems)
            {
                var optimisedItemPath = $"{target}/{item.ItemName}.png";
                DirAccess.MakeDirRecursiveAbsolute(optimisedItemPath.GetBaseDir()).Throw(MakeDirError);

                item.Image.Optimise(out var spritePos, out var shapePos, crop, center)
                    .SavePng(optimisedItemPath).Throw(SaveError);

                yield return new(optimisedItemPath, item.ItemName, center, spritePos, shapePos);

                string SaveError() => $"Error saving {optimisedItemPath}";
                string MakeDirError() => $"Error creating {optimisedItemPath.GetBaseDir()}";
            }
        }
    }
}
