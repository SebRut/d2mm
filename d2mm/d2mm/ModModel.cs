using System;
using System.ComponentModel;
using de.sebastianrutofski.d2mm.Annotations;

namespace de.sebastianrutofski.d2mm
{
    public sealed class ModModel : INotifyPropertyChanged, IComparable
    {
        private readonly Mod _Mod;
        private bool _Activated = false;
        private int _Position = 0;

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

        public Mod Mod
        {
            get { return _Mod; }
        }

        public string Dir
        {
            get { return _Mod.Dir; }
        }

        public bool Activated
        {
            get { return _Activated; }
            set
            {
                if (!Activated.Equals(value))
                {
                    _Activated = value;
                    OnPropertyChanged("Activated");
                }
            }
        }

        public int Position
        {
            get { return _Position; }
            set
            {
                if (!Position.Equals(value))
                {
                    _Position = value;
                    OnPropertyChanged("Position");
                }
            }
        }

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

        public string Version
        {
            get { return _Mod.Version.ToString(); }
            set
            {
                Version newVer = null;
                System.Version.TryParse(value, out newVer);
                if(newVer != null & !_Mod.Version.Equals(newVer))
                {
                    _Mod.Version = newVer;
                    OnPropertyChanged("Version");
                }
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

        public int CompareTo(object obje)
        {
            if (obje is ModModel)
            {
                return Position.CompareTo(((ModModel) obje).Position);
            }
            else
            {
                return 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}