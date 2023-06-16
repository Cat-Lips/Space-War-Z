using System.Runtime.CompilerServices;
using Godot;

namespace GodotSharp.BuildingBlocks
{
    public static class ErrorExtensions
    {
        public static bool NotOk(this Error err) => err is not Error.Ok;

        public static bool NotOk(this Error err, string msg, [CallerFilePath] string caller = null) => err.NotOk(() => msg, caller);
        public static bool NotOk(this Error err, Func<string> msg, [CallerFilePath] string caller = null)
        {
            if (err is Error.Ok) return false;
            GD.Print(err.Msg(msg, caller));
            return true;
        }

        public static void Throw(this Error err, string msg, [CallerFilePath] string caller = null) => err.Throw(() => msg, caller);
        public static void Throw(this Error err, Func<string> msg, [CallerFilePath] string caller = null)
        {
            if (err is Error.Ok) return;

            var _msg = err.Msg(msg, caller);

            GD.Print(_msg);
            //GD.PushError(_msg);
            throw new GDException(err, _msg);
        }

        public static T Throw<T>(this T source, string msg, [CallerFilePath] string caller = null) => source.Throw(() => msg, caller);
        public static T Throw<T>(this T source, Func<string> msg, [CallerFilePath] string caller = null)
        {
            if (source is null)
            {
                var _msg = Msg(msg, caller);

                GD.Print(_msg);
                //GD.PushError(_msg);
                throw new Exception(_msg);
            }

            return source;
        }

        private static string Msg(this Error err, Func<string> msg, string caller)
            => $"{Path.GetFileNameWithoutExtension(caller)}: {msg()} [Error: {err}]";

        private static string Msg(Func<string> msg, string caller)
            => $"{Path.GetFileNameWithoutExtension(caller)}: {msg()}";
    }

    public static class Err
    {
        public static Error Wrap(Action action)
        {
            try
            {
                action();
                return Error.Ok;
            }
            catch (GDException e)
            {
                GD.PushError(e.Message);
                return e.Error;
            }
            catch (Exception e)
            {
                GD.PushError(e.Message);
                return Error.Failed;
            }
        }
    }

    public class GDException : Exception
    {
        public Error Error { get; }

        public GDException(Error err, string msg) : base(msg) => Error = err;
    }
}
