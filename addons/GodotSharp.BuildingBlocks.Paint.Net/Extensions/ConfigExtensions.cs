using FastEnumUtility;
using Godot;
using FileAccess = Godot.FileAccess;

namespace GodotSharp.BuildingBlocks.Paint.Net
{
    public class Config
    {
        private readonly string path;
        private readonly bool saveOnDefault;
        private readonly ConfigFile config = new();

        public Config(string path, bool saveOnDefault = false, bool replace = false)
        {
            this.path = path;
            this.saveOnDefault = saveOnDefault;

            if (!replace && FileAccess.FileExists(path))
                config.Load(path).Throw(LoadError);
        }

        //public string[] Sections() => config.GetSections();
        //public string[] Keys(string section) => config.GetSectionKeys(section);

        public T Get<[MustBeVariant] T>(string section, string key, T dflt) => Get(section, key, () => dflt);
        public T Get<[MustBeVariant] T>(string section, string key, Func<T> dflt = null)
        {
            return config.HasSectionKey(section, key)
                ? config.GetValue(section, key).As<T>()
                : DefaultValue();

            T DefaultValue()
            {
                if (dflt is null) return default;

                var value = dflt();

                if (saveOnDefault)
                {
                    config.SetValue(section, key, Variant.From(value));
                    config.Save(path).Throw(SaveError);
                }

                return value;
            }
        }

        public T GetEnum<T>(string section, string key, T dflt) where T : struct, Enum => GetEnum(section, key, () => dflt);
        public T GetEnum<T>(string section, string key, Func<T> dflt = null) where T : struct, Enum
            => FastEnum.TryParse(Get<string>(section, key, dflt is null ? null : () => FastEnum.ToString(dflt())), out T value) ? value : default;

        private string LoadError() => $"Error loading {path}";
        private string SaveError() => $"Error saving {path}";
    }
}
