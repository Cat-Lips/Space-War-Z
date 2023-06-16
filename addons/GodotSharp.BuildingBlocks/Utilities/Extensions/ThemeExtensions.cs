using Godot;

namespace GodotSharp.BuildingBlocks
{
    public static class ThemeExtensions
    {
        public static void SetFontColor(this Label label, in Color color)
         => label.AddThemeColorOverride("font_color", color);

        public static void ResetFontColor(this Label label)
            => label.RemoveThemeColorOverride("font_color");

        public static void SetMargin(this MarginContainer source, int margin)
            => source.SetMargin(margin, margin, margin, margin);

        public static void SetMargin(this MarginContainer source, int left, int top, int right, int bottom)
        {
            source.AddThemeConstantOverride("margin_top", top);
            source.AddThemeConstantOverride("margin_left", left);
            source.AddThemeConstantOverride("margin_right", right);
            source.AddThemeConstantOverride("margin_bottom", bottom);
        }

        public static void ResetMargin(this MarginContainer source)
        {
            source.RemoveThemeConstantOverride("margin_top");
            source.RemoveThemeConstantOverride("margin_left");
            source.RemoveThemeConstantOverride("margin_right");
            source.RemoveThemeConstantOverride("margin_bottom");
        }
    }
}
