using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    public record SSContent(string SceneName, AnimInfo[] Anims, ItemInfo[] Items, PartInfo[] Parts);

    public static class SceneScraper
    {
        public static bool CanScrapeScene(string path)
            => DirAccess.DirExistsAbsolute(Utils.SSRoot(path));

        public static ICollection<SSContent> ExtractFiles(string path,
            Action<string, float> ReportProgress,
            bool crop = true, bool center = true)
        {
            Dir.Purge(path);

            ReportProgress($"Extracting Files...", 0);
            var content = ExtractFiles();
            ReportProgress("Files Ready", 1);

            return content;

            ICollection<SSContent> ExtractFiles()
            {
                var ssRoot = Utils.SSRoot(path);
                var ssFiles = Dir.EnumerateFiles(ssRoot).ToArray();
                var ssCount = (float)ssFiles.Length;
                var ssScene = (string)null;
                var ssIndex = -1;

                return ssFiles.Select(x => Extract(x.Dir, x.Files)).ToArray();

                SSContent Extract(string dir, string[] files)
                {
                    if (++ssIndex is not 0)
                        ReportProgress(ssScene = dir.GetFile(), ssIndex / ssCount);

                    var anims = ExtractAnims(files.Where(Utils.IsAnimZip));
                    var items = ExtractItems(files.Where(Utils.IsItemsZip));
                    var parts = ExtractParts(files.Where(Utils.IsPartsZip));

                    return new(ssScene, anims, items, parts);

                    AnimInfo[] ExtractAnims(IEnumerable<string> zipFiles)
                    {
                        return zipFiles.Select(ExtractAnim).ToArray();

                        AnimInfo ExtractAnim(string animZip)
                            => ZipAnim.Unpack(animZip).Extract(Utils.AnimTarget(animZip), crop, center);
                    }

                    ItemInfo[] ExtractItems(IEnumerable<string> zipFiles)
                    {
                        return zipFiles.SelectMany(ExtractItems).ToArray();

                        IEnumerable<ItemInfo> ExtractItems(string itemsZip)
                            => ZipItems.Unpack(itemsZip).Extract(Utils.ItemsTarget(itemsZip), crop, center);
                    }

                    PartInfo[] ExtractParts(IEnumerable<string> zipFiles)
                    {
                        return zipFiles.SelectMany(ExtractParts).ToArray();

                        IEnumerable<PartInfo> ExtractParts(string partsZip)
                            => ZipParts.Unpack(partsZip).Extract(Utils.PartsTarget(partsZip), crop, center);
                    }
                }
            }
        }

        public static void ImportFiles(EditorInterface editor, ICollection<SSContent> content)
        {
            var fs = editor.GetResourceFilesystem();
            fs.ReimportFiles(Files().ToArray());

            IEnumerable<string> Files()
            {
                foreach (var (_, anims, items, parts) in content)
                {
                    foreach (var texture in anims.SelectMany(x => x.Frames.Select(x => x.Texture)))
                    {
                        fs.UpdateFile(texture);
                        yield return texture;
                    }

                    foreach (var texture in items.Select(x => x.Texture))
                    {
                        fs.UpdateFile(texture);
                        yield return texture;
                    }

                    foreach (var texture in parts.Select(x => x.Texture))
                    {
                        fs.UpdateFile(texture);
                        yield return texture;
                    }
                }
            }
        }

        public static void ScrapeScene(string path,
            Action<string, float> ReportProgress,
            ICollection<SSContent> content)
        {
            ReportProgress($"Scraping Scene...", 0);
            ScrapeScene();
            ReportProgress($"Scrape Complete", 1);

            void ScrapeScene()
            {
                var ssIndex = -1;
                var ssCount = (float)content.Count;
                var ssBuilder = new SceneBuilder(path);

                foreach (var (sceneName, anims, items, parts) in content)
                {
                    if (++ssIndex is not 0)
                        ReportProgress(sceneName, ssIndex / ssCount);

                    if (parts.Any()) ssBuilder.CreateScene(sceneName, parts, anims);
                    else ssBuilder.CreateAnims(anims);
                    ssBuilder.CreateItems(items);
                }
            }
        }
    }
}
