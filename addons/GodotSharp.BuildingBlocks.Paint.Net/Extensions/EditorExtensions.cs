using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    [Tool]
    public partial class Editor : EditorScript
    {
        public static EditorInterface Interface { get; }
            = new Editor().GetEditorInterface();
    }
}
