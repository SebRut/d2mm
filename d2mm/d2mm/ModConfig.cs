using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace de.sebastianrutofski.d2mm
{
    internal class ModConfig
    {
        private Dictionary<string, object[]> _Configs = new Dictionary<string,object[]>();

        public Dictionary<string,object[]> Configs
        {
            get { return _Configs; }
            set
            {
                if (!Configs.Equals(value))
                {
                    _Configs = value;
                }
            }
        }

        public static ModConfig LoadConfig(string file)
        {
            using (StreamReader reader = File.OpenText(file))
            {
                try
                {
                    return JsonConvert.DeserializeObject<ModConfig>(reader.ReadToEnd());
                }
                catch (Exception ex)
                {
                    return new ModConfig();
                }
            }
        }

        public void SaveConfig(string file)
        {
            string json = JsonConvert.SerializeObject(this);
            if (File.Exists(file)) File.Delete(file);
            using (StreamWriter writer = new StreamWriter(File.OpenWrite(file)))
            {
                writer.Write(json);
            }
        }
    }
}