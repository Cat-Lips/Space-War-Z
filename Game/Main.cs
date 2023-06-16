using Godot;

namespace Game
{
    public partial class Main : Node
    {
        [GodotOverride]
        private void OnReady()
        {
            InitialiseMenu();

            static void InitialiseMenu()
            {

            }
        }

        public override partial void _Ready();
    }
}
