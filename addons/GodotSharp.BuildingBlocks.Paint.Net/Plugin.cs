#if TOOLS
using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    [Tool]
    public partial class Plugin : EditorPlugin
    {
        public Plugin()
        {
            SceneScraperMenu sceneScraperMenu = null;

            TreeEntered += EnablePlugin;
            TreeExiting += DisablePlugin;

            void EnablePlugin()
                => (sceneScraperMenu = new(this)).Attach();

            void DisablePlugin()
            {
                sceneScraperMenu.Detach();
                sceneScraperMenu = null;
            }
        }
    }
}
#endif
