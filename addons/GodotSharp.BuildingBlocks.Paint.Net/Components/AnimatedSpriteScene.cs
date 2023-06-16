using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    [Tool]
    public partial class AnimatedSpriteScene : AnimatedSprite2D
    {
        [GodotOverride]
        private void OnReady()
        {
            var HasRoot = HasMeta(Const.HasRoot);

            var body = GetBody();
            var root = GetRoot();
            var shapes = GetShapes();
            Node2D[] curShape = null;

            FrameChanged += OnFrameChanged;
            AnimationChanged += OnAnimationChanged;

            RigidBody2D GetBody() => HasRoot ? GetParentOrNull<RigidBody2D>() : null;
            Node2D GetRoot() => HasRoot ? GetParentOrNull<CollisionObject2D>() : this;

            Dictionary<string, Node2D[]> GetShapes()
            {
                return HasRoot ? GetShapes() : null;

                Dictionary<string, Node2D[]> GetShapes()
                {
                    return root.GetChildren<Node2D>()
                        .Where(x => x.HasMeta(Const.ShapeKey))
                        .GroupBy(x => (string)x.GetMeta(Const.ShapeKey))
                        .ToDictionary(x => x.Key, x => x.ToArray());
                }
            }

            void OnFrameChanged()
            {
                SetMass();
                SetShape();
                SetOffset();

                void SetMass()
                {
                    if (body is null) return;
                    body.Mass = (float)GetMeta(Utils.AnimKey(Animation, Frame, "Mass"));
                }

                void SetShape()
                {
                    if (shapes is null) return;
                    curShape?.ForEach(x => EnableShape(x, false));
                    curShape = TryGetShape(Animation, Frame);
                    curShape?.ForEach(x => EnableShape(x, true));

                    Node2D[] TryGetShape(StringName animation, int frame)
                        => shapes.TryGetValue(Utils.AnimKey(animation, frame, "Shape"), out var value) ? value : null;

                    void EnableShape(Node2D shape, bool active)
                    {
                        shape.Visible = active;
                        shape.Set("disabled", !active);
                    }
                }

                void SetOffset()
                    => Offset = (Vector2)GetMeta(Utils.AnimKey(Animation, Frame, "Offset"));
            }

            void OnAnimationChanged()
            {
                SetPosition();
                OnFrameChanged();

                void SetPosition()
                    => root.Position = (Vector2)root.GetMeta(Utils.AnimKey(Animation, "Position"));
            }
        }

        public override partial void _Ready();
    }
}
