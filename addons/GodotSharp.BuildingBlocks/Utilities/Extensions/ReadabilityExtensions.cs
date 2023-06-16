using Godot;

namespace GodotSharp.BuildingBlocks
{
    public static class ReadabilityExtensions
    {
        public static void Enabled(this BaseButton node, bool enabled = true)
            => node.Disabled = !enabled;
    }
}
