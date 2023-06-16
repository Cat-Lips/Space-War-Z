using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    internal static class Utils
    {
        private const string AnimZipExt = ".anim.zip";
        private const string ItemsZipExt = ".items.zip";
        private const string PartsZipExt = ".parts.zip";

        public static bool IsAnimZip(string path) => path.EndsWith(AnimZipExt);
        public static bool IsItemsZip(string path) => path.EndsWith(ItemsZipExt);
        public static bool IsPartsZip(string path) => path.EndsWith(PartsZipExt);

        public static string SSRoot(string path) => path.PathJoin(".ss");
        public static string SSTarget(string ssPath) => ssPath.Replace(".ss/", "");
        public static string AnimTarget(string ssPath) => SSTarget(ssPath).TrimSuffix(AnimZipExt);
        public static string ItemsTarget(string ssPath) => SSTarget(ssPath).TrimSuffix(ItemsZipExt);
        public static string PartsTarget(string ssPath) => SSTarget(ssPath).TrimSuffix(PartsZipExt);

        public static string PartKey(StringName part, string type) => $"{part}_{type}";
        public static string AnimKey(StringName anim, string type) => $"{anim}_{type}";
        public static string AnimKey(StringName anim, int frame, string type) => $"{anim}_{frame}_{type}";

        public static Node TryInstantiateScene(string tscn)
            => tscn is null or "" ? default : GD.Load<PackedScene>(tscn)?.Instantiate();

        public static T TryInstantiateScene<T>(string tscn) where T : Node
            => tscn is null or "" ? default : GD.Load<PackedScene>(tscn)?.Instantiate<T>();
    }
}
