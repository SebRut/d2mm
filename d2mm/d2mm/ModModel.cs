using System;
using System.ComponentModel;
using de.sebastianrutofski.d2mm.Annotations;

namespace de.sebastianrutofski.d2mm
{
    public sealed class ModModel : INotifyPropertyChanged
    {
        private readonly Mod _Mod;

        public string Name
        {
            get { return _Mod.Name; }
            set
            {
                if (!Name.Equals(value))
                {
                    _Mod.Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public Version Version
        {
            get { return _Mod.Version; }
            set
            {
                if(!Version.Equals(value))
                {
                    _Mod.Version = value;
                    OnPropertyChanged("Version");
                }
            }
        }

        public string Size
        {
            get
            {
                double converted = _Mod.ModSize.Value;
                string extension = "B";
                if(converted > 99)
                {
                    converted = converted/1024.0;
                    extension = "KB";

                    if (converted > 99)
                    {
                        converted = converted/1024.0;
                        extension = "MB";

                        if (converted > 99)
                        {
                            converted = converted/1024.0;
                            extension = "GB";
                        }
                    }
                } 
                return Math.Round(converted, 2) + " " + extension;
                
            }
        }

        public ModModel(Mod mod)
        {
            this._Mod = mod;
        }


        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}