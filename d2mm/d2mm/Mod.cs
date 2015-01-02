using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace de.sebastianrutofski.d2mm
{
    public class Mod
    {
        private const string ConfigFile = ".d2mm.config";
        private readonly Lazy<long> _ModSize;
        private string _Name = String.Empty;
        private Version _Version = new Version();
        private string _Dir = String.Empty;
        private List<DirMapping> _DirMappings = new List<DirMapping>();

        [JsonIgnore]
        public Lazy<long> ModSize
        {
            get { return _ModSize; }
        }

        public string Name
        {
            get { return _Name; }
            set { 
                if(!Name.Equals(value))
                {
                    _Name = value;
                } 
            }
        }

        [JsonConverter(typeof(ToStringJsonConverter))]
        public Version Version
        {
            get { return _Version; }
            set {
                if (!Version.Equals(value))
                {
                    _Version = value;
                }
            }
        }

        [JsonIgnore]
        public string Dir
        {
            get { return _Dir; }
            set {
                if (!Dir.Equals(value))
                {
                    _Dir = value;
                }; }
        }

        public List<DirMapping> DirMappings
        {
            get { return _DirMappings; }
            set { if(!DirMappings.Equals(value))
                _DirMappings = value;
            }
        }

        public Mod()
        {
            _ModSize = new Lazy<long>(() => Helpers.GetDirectorySize(Dir));
        }

        public static Mod LoadModConfig(string file)
        {
            using (StreamReader reader = File.OpenText(file))
            {
                return JsonConvert.DeserializeObject<Mod>(reader.ReadToEnd());
            }
        }


        public static Mod CreateFromDirectory(string dir)
        {
            Mod mod;

            CreateFromDirectory(dir, out mod);

            return mod;
        }

        public static void CreateFromDirectory(string dir, out Mod mod)
        {
            if (Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).Any(dir.Contains))
            {
                mod = new Mod();
                mod.Name = Path.GetFileName(Path.GetDirectoryName(Path.GetInvalidPathChars().Aggregate(dir, (current,c) => current.Replace(c.ToString(), "~")) + "\\"));
                return;
            }
            string[] files = Directory.GetFiles(dir);
            if (Array.IndexOf(files, Path.Combine(dir, ConfigFile)) > -1)
            {
                try
                {
                    using (StreamReader streamReader = File.OpenText(Path.Combine(dir, ConfigFile)))
                    {
                        CreateFromString(streamReader.ReadToEnd(), out mod);
                    }
                }
                catch (Exception ex)
                {
                    mod = new Mod();
                    mod.Name = Path.GetFileName(Path.GetDirectoryName(dir + "\\"));
                }
            }
            else
            {
                mod = new Mod();
                mod.Name = Path.GetFileName(Path.GetDirectoryName(dir+"\\"));
            }
            mod.Dir = dir;
        }

        public static void CreateFromString(string source, out Mod mod)
        {
            if (source.Equals(String.Empty))
            {
                mod = new Mod();
                return;
            }
            mod = JsonConvert.DeserializeObject<Mod>(source);
        }

        public static Mod CreateFromString(string source)
        {
            Mod mod;

            CreateFromString(source, out mod);

            return mod;
        }

        public void SaveModConfig()
        {
            SaveModConfig(Path.Combine(Dir, ConfigFile));
        }

        public void SaveModConfig(string file)
        {
            string json = JsonConvert.SerializeObject(this);
            if (File.Exists(file)) File.Delete(file);
            using (StreamWriter writer = new StreamWriter(File.OpenWrite(file)))
            {
                writer.Write(json);
            }
        }
    }

    public class ToStringJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (objectType.FullName)
            {
                case "System.Version":
                    return new Version((string) reader.Value);
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}