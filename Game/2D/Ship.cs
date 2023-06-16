using Godot;
using GodotSharp.BuildingBlocks;

namespace Game
{
    [SceneTree]
    public partial class Ship : RigidBody2D
    {
        [GodotOverride]
        private void OnReady()
        {

        }

        [GodotOverride]
        private void OnPhysicsProcess(double delta)
        {
            var turbo = Input.IsActionPressed(GameInput.Turbo);
            var thrust = Input.IsActionPressed(GameInput.ThrustForward);
            var rotateLeft = Input.IsActionPressed(GameInput.RotateLeft);
            var rotateRight = Input.IsActionPressed(GameInput.RotateRight);

            if (rotateLeft)
            {

            }

            if (rotateRight)
            {

            }

            if (thrust)
            {

            }
        }

        [GodotOverride]
        private void OnUnhandledInput(InputEvent e)
        {
            //this.Handle(e,
            //    (GameInput.RotateLeft, RotateLeft),
            //    (GameInput.RotateRight, RotateRight),
            //    (GameInput.ThrustForward, ThrustForward),
            //    (GameInput.FireWeapon, FireWeapon))
            //    (GameInput.SelfDestruct, SelfDestruct));
        }

        public override partial void _Ready();
        public override partial void _PhysicsProcess(double delta);
        public override partial void _UnhandledInput(InputEvent e);
    }
}
