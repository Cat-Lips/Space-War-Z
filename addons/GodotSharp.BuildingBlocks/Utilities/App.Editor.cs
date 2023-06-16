using Godot;
using Godot.Collections;

namespace GodotSharp.BuildingBlocks
{
    public static partial class App
    {
        public static Dictionary PropertyConfig(string name, Variant.Type type, params PropertyUsageFlags[] usage) => new()
        {
            { "name", name },
            { "type", (int)type },
            { "usage", (int)usage.Aggregate((x, y) => x | y) }
        };

        public static Dictionary PropertyConfig(string name, Variant.Type type, PropertyHint hint, string hintString, params PropertyUsageFlags[] usage)
        {
            var cfg = PropertyConfig(name, type, usage);
            cfg.Add("hint", (int)hint);
            cfg.Add("hint_string", hintString);
            return cfg;
        }
    }
}
