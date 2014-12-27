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
    public sealed partial class DirMappingDialog : MetroWindow, INotifyPropertyChanged
    {
        public DirMapping Result;

        public DirMappingDialog(ModModel modModel)
        {
            InitializeComponent();

            List<string> possibleDirs = (from dir in Directory.GetDirectories(modModel.Dir) let found = modModel.DirMappings.Any(dirMappingModel => Path.GetFileName(dir).Equals(dirMappingModel.ModDir)) where !found select Path.GetFileName(dir)).ToList();

            modDirs.ItemsSource = possibleDirs;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
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
