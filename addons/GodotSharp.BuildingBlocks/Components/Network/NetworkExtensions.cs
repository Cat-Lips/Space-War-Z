using Godot;

namespace GodotSharp.BuildingBlocks
{
    public static class NetworkExtensions
    {
        public static bool SinglePlayerMode(this Node node)
            => node.Multiplayer.MultiplayerPeer is OfflineMultiplayerPeer or null;

        public static bool MultiplayerServer(this Node node)
            => !node.SinglePlayerMode() && node.Multiplayer.IsServer();

        public static void AddSpawnableScene<T>(this MultiplayerSpawner source) where T : Node
            => source.AddSpawnableScene(App.GetScenePath<T>());

        public static void SpawnScene(this MultiplayerSpawner source, Node scene)
            => source.GetSpawnNode().AddChild(scene, forceReadableName: true);

        public static Node GetSpawnNode(this MultiplayerSpawner source)
            => source.GetNode(source.SpawnPath);

        public static void DespawnAll(this MultiplayerSpawner source, bool free)
        {
            var spawnParent = source.GetSpawnNode();
            SpawnedItems().ForEach(x => spawnParent.RemoveChild(x, free));

            IEnumerable<Node> SpawnedItems()
            {
                var spawnableScenes = new HashSet<string>(source._SpawnableScenes);
                return spawnParent.GetChildren().Where(x => spawnableScenes.Contains(x.SceneFilePath));
            }
        }
    }
}
