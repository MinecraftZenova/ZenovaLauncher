using ModernWpf.Controls;
using System.Collections.Generic;
using System.Windows;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for ImportModDialog.xaml
    /// </summary>
    public partial class ImportModDialog : ContentDialog
    {
        public ImportModDialog(List<Mod> mods, List<Profile> profiles)
        {
            InitializeComponent();

            ModBox.ItemsSource = mods;
            ProfileBox.ItemsSource = profiles;
            ModSection.Visibility = mods.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            ProfileSection.Visibility = profiles.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            ModBox.Items.Refresh();
            ProfileBox.Items.Refresh();
        }
    }
}
