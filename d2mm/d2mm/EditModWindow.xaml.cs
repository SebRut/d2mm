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
    }
}
