using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    public static class Dir
    {
        public static IEnumerable<string> Files(string path, string pattern = null)
        {
            foreach (var file in DirAccess.GetFilesAt(path))
            {
                if (pattern is null || file.MatchN(pattern))
                    yield return $"{path}/{file}";
            }
        }

        public static void Walk(string path, Action<string[]> action) => Walk(path, null, action);
        public static void Walk(string path, string pattern, Action<string[]> action)
        {
            var files = Files(path, pattern).ToArray();
            if (files.Any()) action(files);

            foreach (var dir in DirAccess.GetDirectoriesAt(path))
                Walk($"{path}/{dir}", pattern, action);
        }

        public static IEnumerable<(string Dir, string[] Files)> EnumerateFiles(string path, string pattern = null)
        {
            var files = Files(path, pattern).ToArray();
            if (files.Any()) yield return (path, files);

            foreach (var dir in DirAccess.GetDirectoriesAt(path))
            {
                foreach (var x in EnumerateFiles($"{path}/{dir}", pattern))
                    yield return x;
            }
        }

        public static void Trash(string path)
        {
            OS.MoveToTrash(ProjectSettings.GlobalizePath(path)).Throw(RemoveError);

            string RemoveError() => $"Error removing {path}";
        }

        public static void Purge(string path)
        {
            foreach (var file in DirAccess.GetFilesAt(path))
            {
                if (!file.StartsWith("."))
                    Trash($"{path}/{file}");
            }

            foreach (var dir in DirAccess.GetDirectoriesAt(path))
            {
                if (!dir.StartsWith("."))
                    Trash($"{path}/{dir}");
            }
        }
    }
}
