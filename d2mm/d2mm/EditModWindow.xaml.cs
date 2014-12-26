using System.IO;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace de.sebastianrutofski.d2mm
{
    /// <summary>
    /// Interaktionslogik für EditModWindow.xaml
    /// </summary>
    public partial class EditModWindow : MetroWindow
    {
        private ModModel _ModModel;

        public ModModel ModModel
        {
            get { return _ModModel; }
            set { _ModModel = value; }
        }

        public EditModWindow(string modDir)
            : this(Mod.CreateFromDirectory(modDir)) {}

        public EditModWindow(Mod mod)
        {
            InitializeComponent();
            ModModel = new ModModel(mod);
            DataContext = ModModel;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ModModel.Mod.SaveModConfig();
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        private void RemoveDirMapping_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DirMappings.SelectedItem != null;
        }

        private void RemoveDirMapping_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ModModel.DirMappings.Remove((DirMappingModel) DirMappings.SelectedItem);
        }

        private void AddDirMapping_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AddDirMapping_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DirMappingDialog dmd = new DirMappingDialog(ModModel);
            dmd.ShowDialog();

            if (dmd.Result != new DirMapping())
            {
                ModModel.DirMappings.Add(new DirMappingModel(dmd.Result));
            }
        }
    }
}
