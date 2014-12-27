using System.Windows;

namespace de.sebastianrutofski.d2mm
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            DLog.WriteLog("log.txt");
        }
    }
}
