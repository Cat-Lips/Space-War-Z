using Godot;

namespace GodotSharp.BuildingBlocks
{
    public static class AsyncExtensions
    {
        public static void CallDeferred(this GodotObject source, Action action)
        {
            Callable.From(() =>
            {
                var invokeAction = GodotObject.IsInstanceValid(source);
                if (invokeAction) action();
            }).CallDeferred();
        }

        public static void CallDeferred(this GodotObject source, CancellationToken ct, Action action, Action<bool> onComplete = null)
        {
            Callable.From(() =>
            {
                var invokeAction = GodotObject.IsInstanceValid(source) && !ct.IsCancellationRequested;
                if (invokeAction) action();
                onComplete?.Invoke(invokeAction);
            }).CallDeferred();
        }
    }
}
