using System.Runtime.CompilerServices;
using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    public static class SceneExtensions
    {
        public static T SetScript<T>(this GodotObject source) where T : GodotObject { source.SetScript<T>(out var obj); return obj; }
        public static void SetScript<T>(this GodotObject source, out T newObj) where T : GodotObject => source.SetScript(App.GetScriptPath<T>(), out newObj);
        public static T SetScript<T>(this GodotObject source, string scriptPath) where T : GodotObject { source.SetScript<T>(scriptPath, out var obj); return obj; }
        public static void SetScript<T>(this GodotObject source, string scriptPath, out T newObj) where T : GodotObject
        {
            var id = source.GetInstanceId();
            source.SetScript(GD.Load<Script>(scriptPath));
            var t = GodotObject.InstanceFromId(id);
            newObj = (T)t;
        }

        public static void Save(this Node tscn, string savePath, string script = null, [CallerFilePath] string caller = null)
        {
            if (script is not null)
                tscn.SetScript(script, out tscn);

            PackedScene scene = new();

            scene.Pack(tscn).Throw(PackError, caller);
            ResourceSaver.Save(scene, savePath).Throw(SaveError, caller);

            string PackError() => $"Error packing {tscn.Name}";
            string SaveError() => $"Error saving {savePath}";
        }
    }
}
