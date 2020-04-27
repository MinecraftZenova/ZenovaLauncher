using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
