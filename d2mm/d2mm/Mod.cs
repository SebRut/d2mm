using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace de.sebastianrutofski.d2mm
{
    public class Mod
    {
        private const string CONFIG_FILE = ".d2mm.config";
        private string _Name = String.Empty;
        private Version _Version = new Version();
        private string _Dir = String.Empty;
        private Lazy<long> _ModSize;

        public Mod()
        {
             _ModSize = new Lazy<long>(() => Helpers.GetDirectorySize(Dir));
        }

        public string Name
        {
            get { return _Name; }
            set { 
                if(!_Name.Equals(value))
                {
                    _Name = value;
                } 
            }
        }

        public Version Version
        {
            get { return _Version; }
            set {
                if (!_Version.Equals(value))
                {
                    _Version = value;
                }
            }
        }

        public string Dir
        {
            get { return _Dir; }
            set {
                if (!Dir.Equals(value))
                {
                    _Dir = value;
                }; }
        }

        public Lazy<long> ModSize
        {
            get { return _ModSize; }
        }

        public static IEnumerable<Mod> LoadRootDirectory(string rootDir)
        {
            List<Mod> result = new List<Mod>();

            foreach (string dir in Directory.GetDirectories(rootDir))
            {
                Mod mod;
                CreateFromDirectory(dir, out mod);
                result.Add(mod);
            }

            return result;
        }

        private static void CreateFromDirectory(string dir, out Mod mod)
        {
            string[] files = Directory.GetFiles(dir);
            if (Array.IndexOf(files, Path.Combine(dir, CONFIG_FILE)) > -1)
            {
                using (StreamReader streamReader = File.OpenText(Path.Combine(dir, CONFIG_FILE)))
                {
                    CreateFromString(streamReader.ReadToEnd(), out mod);
                }
            }
            else
            {
                mod = new Mod();
                mod.Name = Path.GetFileName(Path.GetDirectoryName(dir+"\\"));
            }
            mod.Dir = dir;
        }

        private static void CreateFromString(string source, out Mod mod)
        {
            mod = new Mod();
            JObject result = JObject.Parse(source);

            mod.Name = (string) result["name"];
            mod.Version = Version.Parse((string) result["version"]);
        }

        private static Mod CreateFromDirectory(string dir)
        {
            Mod mod;

            CreateFromDirectory(dir, out mod);

            return mod;
        }

        private static Mod CreateFromString(string source)
        {
            Mod mod;

            CreateFromString(source, out mod);

            return mod;
        }
    }
}