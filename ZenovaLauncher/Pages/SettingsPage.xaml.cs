using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private ObservableCollection<ZenovaUpdater.AssemblyType> Assemblies = new ObservableCollection<ZenovaUpdater.AssemblyType>();

        public SettingsPage()
        {
            InitializeComponent();

            Options.DataContext = Preferences.instance;

            Assemblies.Add(ZenovaUpdater.InstallerAssembly);
            Assemblies.Add(ZenovaUpdater.ApiAssembly);

            AboutPanel.ItemsSource = Assemblies;
        }

        private void FreeSpaceClick(object sender, RoutedEventArgs e)
        {
            VersionManager.instance.RemoveUnusedVersions();
        }
    }
}
