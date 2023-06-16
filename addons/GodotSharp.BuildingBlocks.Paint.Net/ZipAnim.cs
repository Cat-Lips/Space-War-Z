using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    public record ZipFrameInfo(Image Image, string FrameName, string AnimName);
    public record ZipAnimInfo(string RootName, ZipFrameInfo[] Frames);

    public record FrameInfo(string Texture, string FrameName, string AnimName, Vector2 SpritePos, Vector2 ShapePos);
    public record AnimInfo(string RootName, FrameInfo[] Frames, bool Centered);

    public static class ZipAnim
    {
        public static ZipAnimInfo Unpack(string animZip)
        {
            var rootName = animZip.GetFileBaseName();
            var frames = Zip.Read(animZip, x => GetContent(x).ToArray());
            return new(rootName, frames);

            IEnumerable<ZipFrameInfo> GetContent(ZipReader zip)
            {
                foreach (var zipFile in zip.GetFiles().InNaturalOrder())
                {
                    var imageExt = zipFile.GetExtension();
                    if (imageExt is not "png" or "bmp") continue;

                    var image = new Image();
                    var err = image.LoadFromBuffer(zip.ReadFile(zipFile), imageExt);
                    if (err.NotOk(ImageError)) continue;

                    var animName = GetAnimNameFromPath(zipFile);
                    if (animName is null or "") animName = GetAnimNameFromFile(zipFile);
                    if (animName is null or "") animName = rootName.StripDigits();
                    yield return new(image, zipFile.GetFileBaseName(), animName);

                    string ImageError() => $"Error importing {zipFile}";
                }

                string GetAnimNameFromPath(string path)
                    => path.GetBaseDir().Split('\\', '/').Where(x => x != rootName).Join("_").ToPascalCase();

                string GetAnimNameFromFile(string path)
                    => path.GetFileBaseName().TrimPrefixN(rootName).StripDigits("_").ToPascalCase();
            }
        }

        public static AnimInfo Extract(this ZipAnimInfo zipAnim, string target, bool crop, bool center)
        {
            return new(zipAnim.RootName, ExtractFrames().ToArray(), center);

            IEnumerable<FrameInfo> ExtractFrames()
            {
                foreach (var anim in zipAnim.Frames.GroupBy(x => x.AnimName))
                {
                    var animName = anim.Key;
                    var bb = anim.Select(x => x.Image.GetUsedRect()).Aggregate((a, b) => a.Merge(b));

                    foreach (var frame in anim)
                    {
                        var optimisedImagePath = $"{target}/{frame.AnimName}/{frame.FrameName}.png";
                        DirAccess.MakeDirRecursiveAbsolute(optimisedImagePath.GetBaseDir()).Throw(MakeDirError);

                        frame.Image.Optimise(bb, out var spritePos, out var shapePos, crop, center)
                            .SavePng(optimisedImagePath).Throw(SaveError);

                        yield return new(optimisedImagePath, frame.FrameName, frame.AnimName, spritePos, shapePos);

                        string SaveError() => $"Error saving {optimisedImagePath}";
                        string MakeDirError() => $"Error creating {optimisedImagePath.GetBaseDir()}";
                    }
                }
            }
        }
    }
}
