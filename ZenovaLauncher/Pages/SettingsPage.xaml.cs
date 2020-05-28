using System.Windows;
using System.Windows.Controls;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();

            KeepOpenBox.DataContext = Preferences.instance;
            FreeSpaceBox.DataContext = Preferences.instance;
            DebugBox.DataContext = Preferences.instance;
            UpdateBox.DataContext = Preferences.instance;
        }

        private void FreeSpaceClick(object sender, RoutedEventArgs e)
        {
            VersionManager.instance.RemoveUnusedVersions();
        }
    }
}
