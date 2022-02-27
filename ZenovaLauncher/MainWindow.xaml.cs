using MahApps.Metro.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = AccountManager.instance;

            AccountBox.ItemsSource = AccountManager.instance;
            NavView.SelectedItem = NavViewItems.OfType<HamburgerMenuItem>().First();
            Navigate(NavView.SelectedItem);
        }

        private async void AccountChanged(object sender, SelectionChangedEventArgs e)
        {
            await VersionDownloader.user.EnableUserAuthorization();
        }

        private void NavView_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            if (e.IsItemOptions)
            {
                Navigate(NavView.SelectedOptionsItem);
            }
            else
            {
                Navigate(NavView.SelectedItem);
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.SelectedItem = NavViewItems.OfType<HamburgerMenuItem>().FirstOrDefault(x => GetNavigateUri(x) == e.Uri);
            NavView.SelectedOptionsItem = NavViewOptions.OfType<HamburgerMenuItem>().FirstOrDefault(x => GetNavigateUri(x) == e.Uri);

            var selectedItem = NavView.SelectedItem ?? NavView.SelectedOptionsItem;
            if (selectedItem is HamburgerMenuItem item)
            {
                NavView.Header = item.Label;
            }
        }

        private void Navigate(object item)
        {
            if (item is HamburgerMenuItem menuItem)
            {
                Uri navigateUri = GetNavigateUri(menuItem);
                if (ContentFrame.CurrentSource != navigateUri)
                {
                    ContentFrame.Navigate(navigateUri, null, new SuppressNavigationTransitionInfo());
                }
            }
        }

        private Uri GetNavigateUri(HamburgerMenuItemBase item)
        {
            if (item.Tag is Uri uri)
            {
                return uri;
            }
            return new Uri((string)item.Tag, UriKind.Relative);
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Utils.WindowLoaded = true;
            Utils.ShowErrorDialog();
            ModManager.instance.WindowLoaded = true;
            ModManager.instance.TryImportMods(new List<string>());
        }
    }

}
