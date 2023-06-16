using Godot;

namespace GodotSharp.BuildingBlocks
{
    [Tool, SceneTree]
    public partial class ProgressDialog : PopupPanel
    {
        [Notify] public string Message { get => _message.Get(); set => _message.Set(value); }
        [Notify] public float Progress { get => _progress.Get(); set => _progress.Set(value); }
        [Export, Notify] public bool AllowCancel { get => _allowCancel.Get(); set => _allowCancel.Set(value); }

        public event Action CancelButtonPressed;

        public ProgressDialog()
        {
            Ready += () =>
            {
                InitialPosition = Engine.IsEditorHint()
                    ? WindowInitialPosition.Absolute
                    : WindowInitialPosition.CenterMainWindowScreen;

                Size = (Vector2I)Content.Size;
                Content.Resized += () => Size = (Vector2I)Content.Size;

                ProgressText.Text = Message;
                ProgressBar.Value = Progress;
                CancelButton.Visible = AllowCancel;

                MessageChanged += () => ProgressText.Text = Message;
                ProgressChanged += () => ProgressBar.Value = Progress;
                AllowCancelChanged += () => CancelButton.Visible = AllowCancel;

                CancelButton.Pressed += () => CancelButtonPressed?.Invoke();
            };
        }
    }
}
