using Godot;

namespace GodotSharp.BuildingBlocks
{
    public static class TreeExtensions
    {
        public static IEnumerable<T> GetChildren<T>(this Node node) where T : Node
            => node.GetChildren().OfType<T>();

        public static Rect2 GetViewRect(this CanvasItem node)
            => node.GetViewportTransform().AffineInverse() * node.GetViewportRect();

        public static void RemoveChild(this Node parent, Node node, bool free)
        {
            parent.RemoveChild(node);
            if (free) node.QueueFree();
        }
    }
}
