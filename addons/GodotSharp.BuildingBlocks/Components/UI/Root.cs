using Godot;

namespace GodotSharp.BuildingBlocks
{
    [Tool, SceneTree]
    public partial class Root : MarginContainer
    {
        [Export, Notify] public int Margin { get => _margin.Get(); set => _margin.Set(value); }

        private MarginContainer OuterMargin => this;
        private MarginContainer InnerMargin => _.Panel.Margin;

        [GodotOverride]
        private void OnReady()
        {
            InitMargin();

            void InitMargin()
            {
                SetMargin();
                MarginChanged += SetMargin;

                void SetMargin()
                {
                    OuterMargin.SetMargin(Margin);
                    InnerMargin.SetMargin(Margin);
                }
            }
        }

        public override partial void _Ready();
    }
}
