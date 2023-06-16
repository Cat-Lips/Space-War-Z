using Godot;

namespace GodotSharp.BuildingBlocks
{
    [SceneTree]
    public partial class Gravity : Area2D
    {
        public float GravityRadius
        {
            get => ((CircleShape2D)_.Shape.Shape).Radius;
            set => ((CircleShape2D)_.Shape.Shape).Radius = value;
        }
    }
}
