using System.Diagnostics;
using Godot;

namespace GodotSharp.BuildingBlocks
{
    [SceneTree]
    public partial class FlowMenu : Container
    {
        private Control Parent;
        private Popup[] PopupMenus;
        private FlowMenu[] SisterMenus;

        public void ShowMenuItems()
        {
            SetProcess(true);
            MenuItems.Visible = true;
            SisterMenus.ForEach(x => x.HideMenuItems());

        }

        public void HideMenuItems()
        {
            SetProcess(false);
            MenuItems.Visible = false;
        }

        [GodotOverride]
        private void OnReady()
        {
            Debug.Assert(MenuLabel.MouseFilter is not MouseFilterEnum.Ignore);

            Parent = GetParent<Control>();
            PopupMenus = MenuItems.GetChildren()
                .Where(x => x.HasMethod("get_popup"))
                .Select(x => (Popup)x.Call("get_popup"))
                .ToArray();
            SisterMenus = Parent.GetChildren<FlowMenu>()
                .Where(x => x != this)
                .ToArray();

            MenuLabel.MouseEntered += ShowMenuItems;
        }

        [GodotOverride]
        private void OnProcess(double _)
        {
            if (Parent.IsMouseOver()) return;
            if (PopupMenus.Any(x => x.IsMouseOver())) return;

            HideMenuItems();
        }

        public override partial void _Ready();
        public override partial void _Process(double _);
    }
}
