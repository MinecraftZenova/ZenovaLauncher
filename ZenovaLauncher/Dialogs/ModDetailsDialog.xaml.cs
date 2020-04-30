using ModernWpf.Controls;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for ModDetailsDialog.xaml
    /// </summary>
    public partial class ModDetailsDialog : ContentDialog
    {
        public Mod SelectedMod { get; set; }

        public ModDetailsDialog(Mod selectedMod)
        {
            InitializeComponent();
            DataContext = this;
            SelectedMod = selectedMod;
            SelectedMod.SetDescriptionTextBlock(ModDescription);
        }
    }
}
