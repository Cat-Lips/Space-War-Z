using Godot;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    public class SceneBuilder
    {
        private readonly string path;
        private readonly Config config;

        public ShapeType ShapeType { get; set; } = ShapeType.Polygon;
        public float MassMultiplier { get; set; } = 1;

        public float AlphaThreshold { get; set; } = 0; // Godot default: 0.1
        public float VertexEpsilon { get; set; } = 0;  // Godot default: 2
        public int? MinPoints { get; set; } = null;    // Ignore polygons with less points than minPoints; or
        public float? PointRatio { get; set; } = .5f;  // Ignore polygons with less points than largest * pointRatio (0..1) [eg, at .5, ignore shapes less than half size of largest]

        public string AnimRoot { get; set; } = null;
        public string ItemRoot { get; set; } = null;
        public string SceneRoot { get; set; } = null;

        public SceneBuilder(string path)
        {
            config = new Config(Utils.SSRoot(this.path = path).PathJoin(".cfg"), saveOnDefault: true);

            ShapeType = config.GetEnum<ShapeType>(null, "DefaultShapeType");
            MassMultiplier = config.Get<float>(null, "MassMultiplier");
            AlphaThreshold = config.Get<float>(null, "AlphaThreshold");

            AnimRoot = config.Get<string>(null, "AnimRoot");
            ItemRoot = config.Get<string>(null, "ItemRoot");
            SceneRoot = config.Get<string>(null, "SceneRoot");
        }

        public void CreateScene(string sceneName, PartInfo[] parts, AnimInfo[] anims)
        {
            var root = CreateRoot();
            parts.ForEach(AddPart);
            anims.ForEach(AddAnim);

            root.Save($"{path}/{sceneName}.tscn");

            Node CreateRoot()
                => Utils.TryInstantiateScene(SceneRoot) ?? new RigidBody2D { Name = sceneName, UniqueNameInOwner = true };

            void AddPart(PartInfo part)
                => AddComponent(root, part.Texture, part.PartName, part.Visible, part.Centered, part.SpritePos, part.ShapePos);

            void AddAnim(AnimInfo anim)
            {
                if (AnimCount() is 1) AddAnim();
                else Enumerable.Range(1, AnimCount()).ForEach(idx => AddAnim(idx));

                int AnimCount()
                    => config.Get($"{sceneName}.{anim.RootName}", "AnimCount", 1);

                void AddAnim(int? idx = null)
                {
                    AddAnim(out var animRoot);
                    SetAnimPosition();
                    SetAnimAnchor();

                    void AddAnim(out Node2D animRoot)
                    {
                        animRoot = CreateAnim($"{sceneName}.{anim.RootName}", anim);
                        animRoot.Name = $"{animRoot.Name}{idx}";

                        root.AddChild(animRoot, forceReadableName: true);
                        animRoot.Owner = root;
                    }

                    void SetAnimPosition()
                    {
                        foreach (var animName in anim.Frames.Select(x => x.AnimName).Distinct())
                        {
                            var animPos = config.Get($"{sceneName}.{anim.RootName}", $"{animName}.Position{idx}", Vector2.Zero);
                            animRoot.SetMeta(Utils.AnimKey(animName, "Position"), animPos);
                        }
                    }

                    void SetAnimAnchor()
                    {
                        var anchor = config.Get<string>($"{sceneName}.{anim.RootName}", $"AnimAnchor{idx}");
                        if (anchor is null) return;

                        var node = root.GetNodeOrNull(anchor);
                        if (node is null) return;

                        root.MoveChild(animRoot, node.GetIndex());
                    }
                }
            }
        }

        public void CreateAnims(AnimInfo[] anims)
        {
            anims.ForEach(x =>
            {
                var anim = CreateAnim(x.RootName, x);
                anim.Save($"{path}/{x.RootName}.tscn");
            });
        }

        public void CreateItems(ItemInfo[] items)
        {
            items.ForEach(x =>
            {
                var item = CreateItem(x);
                item.Save($"{path}/{x.ItemName}.tscn");
            });

            Node CreateItem(ItemInfo item)
            {
                var root = CreateRoot();
                AddItem(item);
                return root;

                Node CreateRoot()
                    => Utils.TryInstantiateScene(ItemRoot) ?? new RigidBody2D { Name = item.ItemName, UniqueNameInOwner = true };

                void AddItem(ItemInfo item)
                    => AddComponent(root, item.Texture, item.ItemName, visible: true, item.Centered, item.SpritePos, item.ShapePos);
            }
        }

        private Node2D CreateAnim(string configSection, AnimInfo anim)
        {
            var root = CreateRoot();
            var sprite = CreateSprite();
            AddFrames(sprite.SpriteFrames);
            return root;

            Node2D CreateRoot()
            {
                var tscn = config.Get<string>(configSection, "AnimRoot") ?? AnimRoot;
                return Utils.TryInstantiateScene<Node2D>(tscn);
            }

            AnimatedSprite2D CreateSprite()
            {
                var sprite = new AnimatedSprite2D
                {
                    Name = anim.RootName,
                    SpriteFrames = new(),
                    Centered = anim.Centered,
                    Visible = true,
                    UniqueNameInOwner = true,
                }.SetScript<AnimatedSpriteScene>();

                if (root is null)
                {
                    root = sprite;
                    return sprite;
                }

                sprite.Name = "Animation";
                sprite.SetMeta(Const.HasRoot, true);
                root.AddChild(sprite, forceReadableName: true);
                sprite.Owner = root;

                if (root is CollisionObject2D)
                    AddAnimShapes();

                return sprite;

                void AddAnimShapes()
                {
                    foreach (var anim in anim.Frames.GroupBy(x => x.AnimName))
                    {
                        var animName = anim.Key;

                        anim.ForEach((x, idx) =>
                        {
                            var image = LoadTexture(x.Texture).GetImage();
                            var shapeName = Utils.AnimKey(animName, idx, "Shape");
                            var shapes = image.ExtractPolygons(AlphaThreshold, VertexEpsilon, out var mass, MinPoints, PointRatio)
                                .CreateShapes(shapeName, x.ShapePos, active: false, ShapeType);

                            mass *= MassMultiplier;
                            sprite.SetMeta(Utils.AnimKey(animName, idx, "Mass"), mass);

                            shapes.ForEach(shape =>
                            {
                                root.AddChild(shape, forceReadableName: true);
                                shape.Owner = root;

                                shape.SetMeta(Const.ShapeKey, shapeName);
                            });
                        });
                    }
                }
            }

            void AddFrames(SpriteFrames animList)
            {
                var loopAnim = config.Get(configSection, "LoopAnim", true);

                foreach (var anim in anim.Frames.GroupBy(x => x.AnimName))
                {
                    var animName = anim.Key;
                    var animFrames = anim.ToArray();
                    var frameTimes = GetFrameTimes();

                    animList.AddAnimation(animName);
                    animList.SetAnimationLoop(animName, loopAnim);

                    animFrames.ForEach((x, idx) =>
                    {
                        animList.AddFrame(animName, LoadTexture(x.Texture), frameTimes[idx]);
                        sprite.SetMeta(Utils.AnimKey(animName, idx, "Offset"), x.SpritePos);
                    });

                    float[] GetFrameTimes()
                    {
                        if (animFrames.Length is 1) return new float[] { 0 };

                        var frameTimes = config.Get(configSection, $"{animName}.FrameTimes", DefaultFrameTimes);
                        if (frameTimes.Length < animFrames.Length) Array.Resize(ref frameTimes, animFrames.Length);
                        return frameTimes;

                        float[] DefaultFrameTimes() => Enumerable.Repeat(1f, animFrames.Length).ToArray();
                    }
                }
            }
        }

        private void AddComponent(Node root, string _texture, string name, bool visible, bool center, in Vector2 spritePos, in Vector2 shapePos)
        {
            var texture = LoadTexture(_texture);
            var image = texture.GetImage();

            var shapeName = Utils.PartKey(name, "Shape");
            var sprite = texture.CreateSprite(name, spritePos, center, visible).SetScript<SpriteScene>();
            var shapes = image.ExtractPolygons(AlphaThreshold, VertexEpsilon, out var mass, MinPoints, PointRatio)
                .CreateShapes(shapeName, shapePos, visible, ShapeType);

            AddSprite(sprite, mass);
            AddShapes(shapes);

            void AddSprite(SpriteScene sprite, float mass)
            {
                root.AddChild(sprite, forceReadableName: true);
                sprite.Owner = root;

                mass *= MassMultiplier;
                sprite.SetMeta("Mass", mass);

                if (root is RigidBody2D body)
                    body.Mass += mass;
            }

            void AddShapes(IEnumerable<Node2D> shapes)
            {
                foreach (var shape in shapes)
                {
                    root.AddChild(shape, forceReadableName: true);
                    shape.Owner = root;

                    shape.SetMeta(Const.ShapeKey, shapeName);
                }
            }
        }

        private static Texture2D LoadTexture(string resource)
        {
            return GD.Load<Texture2D>(resource).Throw(LoadError);
            string LoadError() => $"Error loading {resource}";
        }
    }
}
