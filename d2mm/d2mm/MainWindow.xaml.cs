using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using de.sebastianrutofski.d2mm.Annotations;
using Newtonsoft.Json.Linq;
using MessageBox = System.Windows.MessageBox;

namespace de.sebastianrutofski.d2mm
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string ConfigFile = ".mods.config";
        private ObservableCollection<ModModel> _Mods = new ObservableCollection<ModModel>();
        private string _ModDir;
        private ModConfig _ModConfig;

        public ObservableCollection<ModModel> Mods
        {
            get { return _Mods; }
            set
            {
                if (!_Mods.Equals(value))
                {
                    _Mods = value;
                    OnPropertyChanged("Mods");
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckSettings();
            Prepare();
            _ModConfig = ModConfig.LoadConfig(ConfigFile);
            LoadMods();
            SortMods();
        }

        private void CheckSettings()
        {
            if (!Properties.Settings.Default.DotaDir.Equals(String.Empty)) return;
            string[] possibleSteamAppsDirs = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "SteamApps",
                    "common"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam",
                    "SteamApps", "common")
            };

            string foundDotaDir = null;

            foreach (string possibleSteamAppsDir in possibleSteamAppsDirs.Where(Directory.Exists))
            {
                string dotaDir;
                if (Directory.Exists(Path.Combine(possibleSteamAppsDir, "dota 2")))
                {
                    dotaDir = Path.Combine(possibleSteamAppsDir, "dota 2");
                }
                else if (Directory.Exists(Path.Combine(possibleSteamAppsDir, "dota 2 beta")))
                {
                    dotaDir = Path.Combine(possibleSteamAppsDir, "dota 2 beta");
                }
                else
                {
                    continue;
                }
                if (MessageBox.Show(String.Format("Detected {0} as DotA 2 directory. Is that right?", dotaDir),
                    "Found!",
                    MessageBoxButton.YesNo).Equals(MessageBoxResult.Yes))
                {
                    foundDotaDir = dotaDir;
                    break;
                }
            }

            if (foundDotaDir == null)
            {
                if (MessageBox.Show("No DotA 2 dir detected. Wanna pick the right one by yourself?",
                    "Not Found!",
                    MessageBoxButton.YesNo).Equals(MessageBoxResult.Yes))
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.ShowDialog();
                    if (fbd.SelectedPath != null)
                    {
                        foundDotaDir = fbd.SelectedPath;
                    }
                }
            }

            if (foundDotaDir == null) return;
            Properties.Settings.Default.DotaDir = foundDotaDir;
            Properties.Settings.Default.Save();
        }

        private void Prepare()
        {
            _ModDir = Path.Combine(Properties.Settings.Default.DotaDir, "mods");
            if (!Directory.Exists(_ModDir)) Directory.CreateDirectory(_ModDir);
            if (!File.Exists(ConfigFile)) new ModConfig().SaveConfig(ConfigFile);
        }

        private void LoadMods()
        {
            foreach (Mod mod in Mod.LoadRootDirectory(_ModDir))
            {
                ModModel mm = new ModModel(mod);
                object[] objects = _ModConfig.Configs[mm.Dir];

                if (objects.Length == 2)
                {
                    mm.Position = Convert.ToInt32(objects[0].ToString());
                    mm.Activated = (bool) objects[1];
                    while (Mods.Any(moda => moda.Position == mm.Position))
                    {
                        mm.Position++;
                    }
                }
                Mods.Add(mm);
            }


        }

        private void SortMods()
        {
            ModModel[] temp = Mods.ToArray();
            Array.Sort(temp);
            Mods = new ObservableCollection<ModModel>(temp);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (ModModel modModel in Mods)
            {
                modModel.Mod.SaveModConfig();
                _ModConfig.Configs[modModel.Dir] = new object[] { modModel.Position, modModel.Activated };
            }
            _ModConfig.SaveConfig(ConfigFile);
        }

        private void MoveModUpButton_Click(object sender, RoutedEventArgs e)
        {
            int selPos = Mods[ModList.SelectedIndex].Position;
            int upperPos = Mods[ModList.SelectedIndex-1].Position;

            Mods[ModList.SelectedIndex].Position = upperPos;
            Mods[ModList.SelectedIndex-1].Position = selPos;
            SortMods();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class GreaterThanToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Int32.Parse(value.ToString()) > Int32.Parse(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
