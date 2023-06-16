using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    [Tool]
    public partial class SpriteScene : Sprite2D
    {
        [GodotOverride]
        private void OnReady()
        {
            var shapes = GetShapes();
            if (shapes?.Length is null or 0) return;

            VisibilityChanged += () =>
            {
                foreach (var shape in shapes)
                {
                    shape.Visible = Visible;
                    shape.Set("disabled", !Visible);
                }
            };

            Node2D[] GetShapes()
            {
                var key = Utils.PartKey(Name, "Shape");
                return GetParent()?.GetChildren<Node2D>()
                    .Where(x => x.HasMeta(Const.ShapeKey) &&
                        (string)x.GetMeta(Const.ShapeKey) == key)
                    .ToArray();
            }
        }

        public override partial void _Ready();
    }
}
