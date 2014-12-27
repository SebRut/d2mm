using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using de.sebastianrutofski.d2mm.Annotations;
using MahApps.Metro.Controls;
using MessageBox = System.Windows.MessageBox;
using System.Diagnostics;

namespace de.sebastianrutofski.d2mm
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
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
                if (!Mods.Equals(value))
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

        private static void CheckSettings()
        {
            d2mm.DLog.Log("Checking Settings...");
            if (Properties.Settings.Default.DotaDir.Length != 0) return;

            string regPath = String.Empty;
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine;
            try
            {
                regKey =
                    regKey.OpenSubKey(
                        @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570", false);
                if (regKey != null)
                {
                    string dir = regKey.GetValue("InstallLocation").ToString();
                    DLog.Log("Possible Dota 2 dir found: " + dir, DLog.LogType.Debug);
                    regPath = Path.Combine(dir, "dota_ugc");
                }
            }
            catch (Exception e)
            {
                d2mm.DLog.Log(e);
            }

            string[] possibleSteamAppsDirs = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "SteamApps",
                    "common"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam",
                    "SteamApps", "common"), 
                regPath
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
                DLog.Log("Possible Dota 2 dir found: " + dotaDir, DLog.LogType.Debug);
                if (MessageBox.Show(String.Format("Detected {0} as DotA 2 directory. Is that right?", dotaDir),
                    "Found!",
                    MessageBoxButton.YesNo).Equals(MessageBoxResult.Yes))
                {
                    DLog.Log("Dota 2 dir set to: " + dotaDir, DLog.LogType.Debug);
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
                        DLog.Log("Dota 2 dir set to: " + fbd.SelectedPath, DLog.LogType.Debug);
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
            DLog.Log("Preparing...");
            DLog.Log("Checking for existing /mods folder");
            _ModDir = Path.Combine(Properties.Settings.Default.DotaDir, "mods");
            if (!Directory.Exists(_ModDir)) Directory.CreateDirectory(_ModDir);
            if (!File.Exists(ConfigFile)) new ModConfig().SaveConfig(ConfigFile);
            
            DLog.Log("Creating FileSystemWatcher for /mods ...");
            FileSystemWatcher modDirWatcher = new FileSystemWatcher
            {
                Path = _ModDir,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            modDirWatcher.Changed += delegate
            {
                LoadMods();
            };
        }

        private void LoadMods()
        {
            DLog.Log("Loading mods...");
            Dispatcher.BeginInvoke(new Action(() => Mods.Clear()));
            foreach (Mod mod in LoadRootDirectory(_ModDir))
            {
                DLog.Log(string.Format("Mod found: {0} - {1}", mod.Name, mod.Dir), DLog.LogType.Debug);
                ModModel mm = new ModModel(mod);
                if(_ModConfig.Configs.ContainsKey(mm.Dir))
                {
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
                }
                Dispatcher.BeginInvoke(new Action(() => Mods.Add(mm)));              
            }
        }

        public static IEnumerable<Mod> LoadRootDirectory(string rootDir)
        {
            List<Mod> result = new List<Mod>();

            foreach (string dir in Directory.GetDirectories(rootDir))
            {
                Mod mod;
                Mod.CreateFromDirectory(dir, out mod);
                result.Add(mod);
            }

            return result;
        }

        private void SortMods()
        {
            DLog.Log("Sorting Mods after position...");
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

        private void ApplyAndStartButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyMods();
            DLog.Log("Starting Dota 2...");
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("steam://rungameid/570", "-override_vpk");
            p.Start();

            while (Process.GetProcessesByName("dota").Length == 0)
            {
                Thread.Sleep(1000);
            }
            Process.GetProcessesByName("dota")[0].WaitForExit();

            RemoveMods();
        }

        private void ApplyMods()
        {
            RemoveMods();
            DLog.Log("Applying mods...");
            foreach (ModModel modModel in Mods.Where(mm => mm.Activated))
            {
                foreach (DirMapping dm in modModel.Mod.DirMappings)
                {
                    CopyDirectoryToDirectory(Path.Combine(modModel.Dir, dm.ModDir),Path.Combine(Properties.Settings.Default.DotaDir, dm.DotaDir));
                }
            }
        }

        private void RemoveMods(bool uncheck = false)
        {
            DLog.Log("Removing mods...");
            foreach (ModModel modModel in Mods.Where(mm => mm.Activated))
            {
                foreach (DirMapping dm in modModel.Mod.DirMappings)
                {
                    DeleteDirectoryFromDirectory(Path.Combine(modModel.Dir, dm.ModDir), Path.Combine(Properties.Settings.Default.DotaDir, dm.DotaDir));
                }

                modModel.Activated = !uncheck;
            }
        }

        private void DeleteDirectoryFromDirectory(string removableDir, string cleanableDir)
        {
            DLog.Log("Deleting dir...");
            try
            {
                foreach (string file in Directory.GetFiles(removableDir))
                {
                    if (!Path.GetInvalidPathChars().Any(invalidFileNameChar => file.Contains(invalidFileNameChar)) &
                        File.Exists(Path.Combine(cleanableDir, Path.GetFileName(file))))
                        DLog.Log("Deleting file: " + Path.Combine(cleanableDir, Path.GetFileName(file)), DLog.LogType.Debug);
                        File.Delete(Path.Combine(cleanableDir, Path.GetFileName(file)));
                }

                if (!Directory.GetDirectories(cleanableDir).Any() && !Directory.GetFiles(cleanableDir).Any())
                {
                    DLog.Log("Deleting dir: " + cleanableDir, DLog.LogType.Debug);
                    Directory.Delete(cleanableDir);
                }

            }
            catch (Exception ex)
            {
                DLog.Log(ex);
            }

            if (!Directory.Exists(removableDir))
                return;

            foreach (string directory in Directory.GetDirectories(removableDir))
            {
                if (!Path.GetInvalidPathChars().Any(c => directory.Contains(c)))
                {
                    if (directory != null)
                        DeleteDirectoryFromDirectory(directory,
                            Path.Combine(cleanableDir,
                                Path.GetFileName(Path.GetDirectoryName(string.Format("{0}{1}", directory, Path.DirectorySeparatorChar)))));
                }
            }
        }

        private void CopyDirectoryToDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
            if (Directory.Exists(sourceDir))
            {
                foreach (string file in Directory.GetFiles(sourceDir))
                {
                    try
                    {
                        DLog.Log(
                            string.Format("Copying file {0} to {1}", file, Path.Combine(destDir, Path.GetFileName(file))),
                            DLog.LogType.Debug);
                        File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
                    }
                    catch (Exception ex)
                    {
                        DLog.Log(ex);
                    }
                }
            }

            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                CopyDirectoryToDirectory(directory, Path.Combine(destDir, Path.GetFileName(Path.GetDirectoryName(directory+"\\"))));
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveMods(true);
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyMods();
        }

        private void EditModButton_Click(object sender, RoutedEventArgs e)
        {
            new EditModWindow(((ModModel)ModList.SelectedItem).Mod).ShowDialog();
        }

        private void NewModButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK & Directory.Exists(fbd.SelectedPath))
            {
                new EditModWindow(fbd.SelectedPath).ShowDialog();
            }
            
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
