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

            DataContext = ProfileLauncher.instance;
            ProfileBox.ItemsSource = ProfileManager.instance;
            ProfileBox.DataContext = ProfileManager.instance;
        }

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            ProfileLauncher.instance.LaunchProfile(ProfileBox.SelectedItem as Profile);
        }

        private void SelectedProfileChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileBox.SelectedItem == null)
                ProfileBox.SelectedIndex = 0;
        }
    }
}
