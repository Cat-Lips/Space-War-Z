using Godot;

namespace GodotSharp.BuildingBlocks
{
    [Tool]
    public partial class Background : ParallaxBackground
    {
        [Export] private Texture2D[] Layers { get; set; }

        [GodotOverride]
        private void OnReady()
        {
            var count = Layers?.Length ?? 0;
            for (var idx = 0; idx < count; ++idx)
            {
                var texture = Layers[idx];
                var size = texture.GetSize();
                var scroll = Vector2.One * idx / count;
                var sprite = new Sprite2D { Texture = texture, Centered = false };
                var layer = new ParallaxLayer { MotionScale = scroll, MotionMirroring = size };

                AddChild(layer);
                layer.AddChild(sprite);
            };
        }

        public override partial void _Ready();
    }
}
