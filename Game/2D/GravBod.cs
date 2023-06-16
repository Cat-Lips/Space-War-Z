using Godot;

namespace Game
{
    [Tool, SceneTree]
    public partial class GravBod : AnimatableBody2D
    {
        [GodotOverride]
        private void OnPhysicsProcess(double delta)
            => Rotate(ConstantAngularVelocity * (float)delta);

        public override partial void _PhysicsProcess(double delta);
    }
}
