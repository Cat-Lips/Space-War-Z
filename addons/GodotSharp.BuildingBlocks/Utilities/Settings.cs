using Godot;

namespace GodotSharp.BuildingBlocks
{
    public class Settings<T>(bool autoSave = true) : Settings(typeof(T).Name, autoSave) { }

    public class Settings
    {
        private readonly ConfigFile config = new();

        private readonly string path;
        private readonly bool autosave;

        public Settings(string name, bool _autosave = true)
        {
            autosave = _autosave;
            path = $"user://{name}.cfg";

            Load();
        }

        public void Set<[MustBeVariant] T>(string key, T value) => Set("", key, value);
        public void Set<[MustBeVariant] T>(string section, string key, T value)
        {
            config.SetValue(section, key, Variant.From(value));
            if (autosave) Save();
        }

        public bool TryGet<[MustBeVariant] T>(string key, out T value) => TryGet("", key, out value);
        public bool TryGet<[MustBeVariant] T>(string section, string key, out T value)
        {
            if (config.HasSectionKey(section, key))
            {
                value = config.GetValue(section, key).As<T>();
                return true;
            }

            value = default;
            return false;
        }

        public T TryGet<[MustBeVariant] T>(string key) => TryGet<T>("", key);
        public T TryGet<[MustBeVariant] T>(string section, string key)
            => TryGet<T>(section, key, out var value) ? value : value;

        public T TryGet<[MustBeVariant] T>(string key, T @default) => TryGet<T>("", key, @default);
        public T TryGet<[MustBeVariant] T>(string section, string key, T @default)
            => TryGet<T>(section, key, out var value) ? value : @default;

        public void Load()
            //=> config.LoadEncryptedPass(Path, App.Name);
            => config.Load(path);

        public void Save()
            //=> config.SaveEncryptedPass(Path, App.Name);
            => config.Save(path);

        public void Clear()
            => config.Clear();

        public string[] Keys() => Keys("");
        public string[] Keys(string section)
            => config.GetSectionKeys(section);
    }
}
