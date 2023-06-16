using Godot;
using GodotSharp.BuildingBlocks;

namespace Game
{
    [SceneTree]
    public partial class SpaceWar : Node2D
    {
        [Export] private PackedScene[] Ships { get; set; }
        [Export] private PackedScene[] GravBods { get; set; }
        [Export] private PackedScene[] Explosions { get; set; }

        [GodotOverride]
        private void OnReady()
        {
            var gameArea = this.GetViewRect();
            var gravBod = SpawnGravBod();
            var ships = SpawnShips();
            ConfigureGravity();

            Node2D SpawnGravBod()
            {
                var gravBod = GravBods.PickRandom().Instantiate<Node2D>();
                gravBod.Position = gameArea.GetCenter();
                AddChild(gravBod);
                return gravBod;
            }

            Node2D[] SpawnShips()
            {
                return SpawnShips().ToArray();

                IEnumerable<Node2D> SpawnShips()
                {
                    var spawnRadius = Math.Min(gameArea.Size.X, gameArea.Size.Y) * .5f;
                    var spawnArea = new Circle(gravBod.Position, spawnRadius);
                    var spawnPoints = spawnArea.Slice(Ships.Length).Randomise().ToArray();

                    for (var i = 0; i < Ships.Length; ++i)
                    {
                        var ship = Ships[i].Instantiate<Node2D>();
                        ship.Position = spawnPoints[i];
                        ship.LookAt(gravBod.Position);
                        ship.Rotate(Const.HalfCircle);
                        AddChild(ship);
                        yield return ship;
                    }
                }
            }

            void ConfigureGravity()
            {
                Gravity.Gravity = 10;
                Gravity.GravityRadius = Math.Min(gameArea.Size.X, gameArea.Size.Y) * .5f;
            }
        }

        public override partial void _Ready();
    }
}
