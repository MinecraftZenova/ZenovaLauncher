using System.Windows;
using System.Windows.Controls;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for PlayPage.xaml
    /// </summary>
    public partial class PlayPage : Page
    {
        public PlayPage()
        {
            InitializeComponent();
            ProfileBox.ItemsSource = ProfileManager.instance;
            DataContext = ProfileLauncher.instance;
        }

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            ProfileLauncher.instance.LaunchProfile(ProfileBox.SelectedItem as Profile);
        }
    }
}
