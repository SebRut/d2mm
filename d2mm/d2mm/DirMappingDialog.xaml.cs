using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using de.sebastianrutofski.d2mm.Annotations;
using MahApps.Metro.Controls;
using Path = System.IO.Path;

namespace de.sebastianrutofski.d2mm
{
    /// <summary>
    /// Interaktionslogik für DirMappingDialog.xaml
    /// </summary>
    public partial class DirMappingDialog : MetroWindow, INotifyPropertyChanged
    {
        public DirMapping Result;

        public DirMappingDialog(ModModel modModel)
        {
            InitializeComponent();

            List<string> possibleDirs = new List<string>();
            foreach (
                string dir in
                    Directory.GetDirectories(modModel.Dir))
            {
                bool found = false;
                foreach (DirMappingModel dirMappingModel in modModel.DirMappings)
                {
                    if (dir.Contains(dirMappingModel.ModDir))
                    {
                        found = true;
                        break;
                    }
                }
                //bool found = modModel.DirMappings.Any(dirMappingModel => dir.Contains(dirMappingModel.DotaDir));
                if (!found)
                    possibleDirs.Add(Path.GetFileName(Path.GetDirectoryName(dir)));
            }

            modDirs.ItemsSource = possibleDirs;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Result = new DirMapping();
            Result.DotaDir = dotaDir.Text;
            Result.ModDir = modDirs.SelectedValue as string;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
