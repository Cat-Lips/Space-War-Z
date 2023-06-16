using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    public static class Zip
    {
        public static T Read<T>(string zipFile, Func<ZipReader, T> action)
        {
            using (var zip = new ZipReader())
            {
                zip.Open(zipFile).Throw(OpenError);
                try { return action(zip); }
                finally { zip.Close().Throw(CloseError); }
            }

            string OpenError() => $"Error opening {zipFile}";
            string CloseError() => $"Error closing {zipFile}";
        }

        public static void Read(string zipFile, Action<ZipReader> action)
            => Read<object>(zipFile, zip => { action(zip); return null; });
    }
}
