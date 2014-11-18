using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace de.sebastianrutofski.d2mm
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<ModModel> _Mods = new ObservableCollection<ModModel>();
        private string _ModDir;

        public ObservableCollection<ModModel> Mods
        {
            get { return _Mods; }
            set
            {
                if (!_Mods.Equals(value))
                {
                    _Mods = value;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = Mods;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckConfig();
            Prepare();
            LoadMods();
        }

        private void CheckConfig()
        {
            if (Properties.Settings.Default.DotaDir.Equals(String.Empty))
            {
                string[] possibleSteamAppsDirs = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "SteamApps",
                        "common"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam",
                        "SteamApps", "common")
                };

                string foundDotaDir = null;

                foreach (string possibleSteamAppsDir in possibleSteamAppsDirs)
                {
                    if (Directory.Exists(possibleSteamAppsDir))
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
        }

        private void Prepare()
        {
            _ModDir = Path.Combine(Properties.Settings.Default.DotaDir, "mods");
            if (!Directory.Exists(_ModDir)) Directory.CreateDirectory(_ModDir);
        }

        private void LoadMods()
        {
            foreach (Mod mod in Mod.LoadRootDirectory(_ModDir))
            {
                Mods.Add(new ModModel(mod));
            }
        }
    }

    internal class Helpers
    {
        public static long GetDirectorySize(string dir)
        {
            long size = 0;

            foreach (string file in Directory.GetFiles(dir))
            {
                size += new FileInfo(file).Length;
            }

            foreach (string subDir in Directory.GetDirectories(dir))
            {
                size += GetDirectorySize(subDir);
            }

            return size;
        }
    }
}
