using AsyncAwaitBestPractices;
using Godot;

namespace GodotSharp.BuildingBlocks
{
    public static partial class App
    {
        public static void RunAsync(Action action)
            => Task.Run(action).SafeFireAndForget();

        public static void CallDeferred(Action action)
            => Callable.From(action).CallDeferred();

        public static bool IsOnMainThread()
            => OS.GetThreadCallerId() == OS.GetMainThreadId();

        public static void RunOnMainThread(Action action)
        {
            if (IsOnMainThread()) { action(); return; }

            var waitHandle = new ManualResetEvent(false);
            CallDeferred(() => { try { action(); } finally { waitHandle.Set(); } });
            waitHandle.WaitOne();
        }

        public static T RunOnMainThread<T>(Func<T> action)
        {
            if (IsOnMainThread()) return action();

            T result = default;
            var waitHandle = new ManualResetEvent(false);
            CallDeferred(() => { try { result = action(); } finally { waitHandle.Set(); } });
            waitHandle.WaitOne();
            return result;
        }

        public static void RunParallel(params Action[] actions)
            => Parallel.ForEach(actions, action => action());

        public static void RunParallel<T>(Action<T> action, params T[] args)
            => Parallel.ForEach(args, action);

        public static CancellationTokenSource RunAsync(Action<CancellationToken> action)
        {
            var cs = new CancellationTokenSource();
            Task.Run(() => action(cs.Token)).SafeFireAndForget();
            return cs;
        }
    }
}
