using Godot;

namespace GodotSharp.BuildingBlocks
{
    public static partial class App
    {
        public static readonly string Name
            = (string)ProjectSettings.GetSetting("application/config/name");

        public static readonly int Hash = GD.Hash(Name);
    }
}
