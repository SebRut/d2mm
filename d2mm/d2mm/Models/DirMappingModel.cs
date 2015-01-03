using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using de.sebastianrutofski.d2mm.Annotations;

namespace de.sebastianrutofski.d2mm
{
    public sealed class DirMappingModel : INotifyPropertyChanged
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
        private void OnPropertyChanged(string propertyName)
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