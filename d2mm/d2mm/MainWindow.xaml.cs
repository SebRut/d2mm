using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using de.sebastianrutofski.d2mm.Annotations;
using MahApps.Metro.Controls;
using MessageBox = System.Windows.MessageBox;
using MahApps.Metro.Controls.Dialogs;

namespace de.sebastianrutofski.d2mm
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : MetroWindow, INotifyPropertyChanged
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
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckSettings();
            Prepare();
            _ModConfig = ModConfig.LoadConfig(ConfigFile);
            LoadMods();
            SortMods();
        }

        private async Task<bool> CheckSettings()
        {
            DLog.Log("Checking Settings...");
            if (Properties.Settings.Default.DotaDir.Length != 0) return false;

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
                DLog.Log(e);
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
                MessageDialogResult r = await this.ShowMessageAsync("Found!",
                    String.Format("Detected {0} as DotA 2 directory. Is that right?", dotaDir)
                    , MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings() {AffirmativeButtonText = "Yes", NegativeButtonText = "No"});
                if (r == MessageDialogResult.Affirmative)
                {
                    DLog.Log("Dota 2 dir set to: " + dotaDir, DLog.LogType.Debug);
                    foundDotaDir = dotaDir;
                    break;
                }
            }

            if (foundDotaDir == null)
            {
                MessageDialogResult r = await this.ShowMessageAsync("Not Found!",
                    "No DotA 2 dir detected. Wanna pick the right one by yourself?"
                    , MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings() { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });
                if( r == MessageDialogResult.Affirmative)
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

            if (foundDotaDir == null) return false;
            Properties.Settings.Default.DotaDir = foundDotaDir;
            Properties.Settings.Default.Save();

            return true;
        }

        private void Prepare()
        {
            DLog.Log("Preparing...");
            DLog.Log("Checking for existing /mods folder");
            _ModDir = Path.Combine(Properties.Settings.Default.DotaDir, "mods");
            if (!Directory.Exists(_ModDir)) Directory.CreateDirectory(_ModDir);
            if (!File.Exists(ConfigFile)) new ModConfig().SaveConfig(ConfigFile);

            DLog.Log("Creating FileSystemWatcher for /mods ...");
        }

        private async void LoadMods()
        {
            ProgressDialogController pdc = await this.ShowProgressAsync("Loading mods...", String.Empty);
            pdc.SetIndeterminate();
            pdc.SetCancelable(false);

            DLog.Log("Loading mods...");
            Mods.Clear();


            pdc.SetMessage("Loading " + Directory.GetDirectories(_ModDir).Length + " mods. Please wait.");

            foreach (Mod mod in Mod.LoadRootDirectory(_ModDir))
            {
                DLog.Log(string.Format("Mod found: {0} - {1}", mod.Name, mod.Dir), DLog.LogType.Debug);
                ModModel mm = new ModModel(mod);
                if (_ModConfig.Configs.ContainsKey(mm.Dir))
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
                Mods.Add(mm);
            }

            await pdc.CloseAsync();
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
                _ModConfig.Configs[modModel.Dir] = new object[] {modModel.Position, modModel.Activated};
            }
            _ModConfig.SaveConfig(ConfigFile);
        }

        private void MoveModUpButton_Click(object sender, RoutedEventArgs e)
        {
            int selPos = Mods[ModList.SelectedIndex].Position;
            int upperPos = Mods[ModList.SelectedIndex - 1].Position;

            Mods[ModList.SelectedIndex].Position = upperPos;
            Mods[ModList.SelectedIndex - 1].Position = selPos;
            SortMods();
        }

        private async void ApplyAndStartButton_Click(object sender, RoutedEventArgs e)
        {
            await ApplyMods();
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

        private async Task<bool> ApplyMods()
        {
            RemoveMods();
            IEnumerable<ModModel> activatedMods = Mods.Where(mm => mm.Activated);
            var pdc = await this.ShowProgressAsync("Installing mods...", "Installing " + activatedMods.ToArray<ModModel>().Length + " mods. Please wait.");
            pdc.SetIndeterminate();
            pdc.SetCancelable(false);

            DLog.Log("Applying mods...");

            
            foreach (ModModel modModel in activatedMods)
            {
                DLog.Log("Installing mod \"" + modModel.Name + "\"", DLog.LogType.Debug);
                foreach (DirMapping dm in modModel.Mod.DirMappings)
                {
                    Helpers.CopyDirectoryToDirectory(Path.Combine(modModel.Dir, dm.ModDir),
                        Path.Combine(Properties.Settings.Default.DotaDir, dm.DotaDir));
                }
            }

            await pdc.CloseAsync();
            return true;
        }

        private void RemoveMods(bool uncheck = false)
        {
            DLog.Log("Removing mods...");
            foreach (ModModel modModel in Mods.Where(mm => mm.Activated))
            {
                foreach (DirMapping dm in modModel.Mod.DirMappings)
                {
                    Helpers.DeleteDirectoryFromDirectory(Path.Combine(modModel.Dir, dm.ModDir),
                        Path.Combine(Properties.Settings.Default.DotaDir, dm.DotaDir));
                }

                modModel.Activated = !uncheck;
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveMods(true);
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            await ApplyMods();
        }

        private void EditModButton_Click(object sender, RoutedEventArgs e)
        {
            new EditModWindow(((ModModel) ModList.SelectedItem).Mod).ShowDialog();
        }

        private void NewModButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK & Directory.Exists(fbd.SelectedPath))
            {
                new EditModWindow(fbd.SelectedPath).ShowDialog();
            }

        }

        private void ImportFromZipButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                Filter = "Zip files|*.zip"
            };

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImportMod(ofd.FileName);
            }

        }

        private void ImportMod(string fileName)
        {
            DLog.Log("Importing mod from " + fileName, DLog.LogType.Debug);
            string optDirName = Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant().Replace(" ", "_");

            optDirName = Path.GetInvalidFileNameChars().Aggregate(optDirName, (current, c) => current.Replace(c.ToString(), "~"));

            string destDir = Path.Combine(_ModDir, optDirName);

            try
            {
                DLog.Log("Unzipping mod to " + destDir);
                ZipFile.ExtractToDirectory(fileName, destDir);
            }
            catch (Exception e)
            {
                DLog.Log(e);
                return;
            }
            Mod mod = Mod.CreateFromDirectory(destDir);

            if (Directory.GetDirectories(destDir).Any(d => d == "dota"))
            {
                foreach (string dir in Directory.GetDirectories(Path.Combine(destDir, "/dota")))
                {
                    string dirName = Path.GetFileName(Path.GetDirectoryName(dir + Path.DirectorySeparatorChar));
                    Helpers.MoveDirectoryToDirectory(Path.Combine(destDir, "/dota", Path.DirectorySeparatorChar + dirName),
                        Path.Combine(destDir, Path.DirectorySeparatorChar + dirName));
                    mod.DirMappings.Add(new DirMapping(dir, Path.Combine("dota", Path.DirectorySeparatorChar + dirName)));
                }
            }
            List<String> dotaDirs = Directory.GetDirectories(Path.Combine(Properties.Settings.Default.DotaDir + Path.DirectorySeparatorChar, "dota")).Select(dir => Path.GetFileName(Path.GetDirectoryName(Path.Combine(dir + Path.DirectorySeparatorChar)))).ToList();
            foreach (string dir in Directory.GetDirectories(destDir).Select(dir => Path.GetFileName(Path.GetDirectoryName(dir + Path.DirectorySeparatorChar))))
            {
                if (!mod.DirMappings.Any(dm => dm.ModDir.Equals(dir)) & (dotaDirs.Any(dd => dd.Equals(dir))))
                {
                    mod.DirMappings.Add(new DirMapping(dir, ("dota" + Path.PathSeparator + dir)));
                }
            }

            mod.SaveModConfig();
            Mods.Add(new ModModel(mod));
        }

        private void ReloadModsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMods();
        }

        private async void DeleteModButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialogResult r = await this.ShowMessageAsync("Delete!",
                    String.Format("Do you really want to delete {0}?", ((ModModel)ModList.SelectedItem).Name)
                    , MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings() {AffirmativeButtonText = "Yes", NegativeButtonText = "No"});
            if (r == MessageDialogResult.Affirmative)
            {
                DLog.Log("Deleting mod " + ModList.SelectedItem, DLog.LogType.Debug);
                bool exceptioned = false;
                try
                {
                    Directory.Delete(((ModModel) ModList.SelectedItem).Dir, true);
                }
                catch (Exception ex)
                {
                    DLog.Log(ex);
                    exceptioned = true;
                }
                if (exceptioned)
                {
                    await
                        this.ShowMessageAsync("Error!",
                            "an error encountered while deleting the mod's dir. You may have to delete it manually");
                }

                Mods.Remove((ModModel)ModList.SelectedItem);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
