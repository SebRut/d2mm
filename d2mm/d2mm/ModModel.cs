using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using de.sebastianrutofski.d2mm.Annotations;

namespace de.sebastianrutofski.d2mm
{
    public sealed class ModModel : INotifyPropertyChanged, IComparable
    {
        private readonly Mod _Mod;
        private bool _Activated = false;
        private int _Position = 0;
        private ObservableCollection<DirMappingModel> _DirMappings;

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
            get
            {
                _Mod.DirMappings = new List<DirMapping>();
                foreach (DirMappingModel dirMappingModel in DirMappings)
                {
                    _Mod.DirMappings.Add(dirMappingModel.DirMapping);
                }
                return _Mod;
            }
        }

        public string Dir
        {
            get { return _Mod.Dir; }
        }

        public ObservableCollection<DirMappingModel> DirMappings
        {
            get { return _DirMappings; }
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
            _DirMappings = DirMappingModel.FromDirMappingList(mod.DirMappings);
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

    public class DirMappingModel : INotifyPropertyChanged
    {
        private DirMapping _DirMapping;

        public DirMapping DirMapping
        {
            get { return _DirMapping; }
        }

        public string ModDir
        {
            get { return _DirMapping.ModDir; }
            set
            {
                if(!ModDir.Equals(value))
                {
                    _DirMapping.ModDir = value;
                    OnPropertyChanged("ModDir");
                }
            }
        }

        public string DotaDir
        {
            get { return _DirMapping.DotaDir; }
            set
            {
                if (!DotaDir.Equals(value))
                {
                    _DirMapping.DotaDir = value;
                    OnPropertyChanged("DotaDir");
                }
            }
        }

        public DirMappingModel(DirMapping dirMapping)
        {
            _DirMapping = dirMapping;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public static ObservableCollection<DirMappingModel> FromDirMappingList(List<DirMapping> dirMappings)
        {
            ObservableCollection<DirMappingModel> result = new ObservableCollection<DirMappingModel>();

            foreach (DirMapping dirMapping in dirMappings)
            {
            
                result.Add(new DirMappingModel(dirMapping));

            }

            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}