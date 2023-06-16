using Godot;

namespace GodotSharp.BuildingBlocks
{
    public partial class Camera : Camera2D
    {
        [Export] public float ZoomMin { get; set; } = .1f; //.5f;
        [Export] public float ZoomMax { get; set; } = 5; //2;
        [Export] public float ZoomFactor { get; set; } = .1f;
        [Export] public float ZoomDuration { get; set; } = .2f;
        [Notify] public float ZoomTarget { get => _zoomTarget.Get(); set => _zoomTarget.Set(Math.Clamp(value, ZoomMin, ZoomMax)); }

        [Export] public StringName ZoomIn { get; set; } = "ZoomIn";
        [Export] public StringName ZoomOut { get; set; } = "ZoomOut";

        public Camera()
            => ZoomTarget = 1;

        [GodotOverride]
        private void OnReady()
        {
            Tween tween = null;
            ResetZoomTween();
            ZoomTargetChanged += ResetZoomTween;

            void ResetZoomTween()
            {
                tween?.Kill();
                tween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
                tween.TweenProperty(this, "zoom", new Vector2(ZoomTarget, ZoomTarget), ZoomDuration);
                tween.TweenCallback(Callable.From(() => tween = null));
            }
        }

        [GodotOverride]
        private void OnUnhandledInput(InputEvent e)
        {
            if (e.IsActionPressed(ZoomIn)) ZoomTarget -= ZoomFactor;
            if (e.IsActionPressed(ZoomOut)) ZoomTarget += ZoomFactor;
        }

        public override partial void _Ready();
        public override partial void _UnhandledInput(InputEvent e);
    }
}
