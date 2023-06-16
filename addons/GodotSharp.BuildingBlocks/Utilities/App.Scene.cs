using System.Reflection;
using Godot;

namespace GodotSharp.BuildingBlocks
{
    public static partial class App
    {
        public static T InstantiateScene<T>() where T : Node
            => LoadScene<T>().Instantiate<T>();

        public static PackedScene LoadScene<T>() where T : Node
            => GD.Load<PackedScene>(GetScenePath<T>());

        public static string GetScenePath<T>() where T : GodotObject
            => GetScriptPath<T>().Replace(".cs", ".tscn");

        public static string GetScriptPath<T>() where T : GodotObject
            => typeof(T).GetCustomAttribute<ScriptPathAttribute>(false).Path;

        public static string GetScriptDir<T>() where T : GodotObject
            => GetScriptPath<T>().GetBaseDir();

        public static T InstantiateScene<T>(string name) where T : Node
        {
            var t = InstantiateScene<T>();
            t.Name = name;
            return t;
        }
    }
}
