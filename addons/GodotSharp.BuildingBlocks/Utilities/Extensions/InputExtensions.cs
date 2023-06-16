using Godot;

namespace GodotSharp.BuildingBlocks
{
    public static class InputExtensions
    {
        public static bool IsMouseOver(this Control source)
        => source.GetRect().HasPoint(source.GetLocalMousePosition());

        public static bool IsMouseOver(this Popup source)
            => source.Visible && new Rect2(default, source.Size).HasPoint(source.GetMousePosition());
    }
}
