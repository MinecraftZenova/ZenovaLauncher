using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for ModsPage.xaml
    /// </summary>
    public partial class ModsPage : Page
    {
        public ModsPage()
        {
            InitializeComponent();

            DataContext = ProfileLauncher.instance;
            ModsListBox.ItemsSource = ModManager.instance;
            SortModsBox.DataContext = Preferences.instance;

            SortModList(Preferences.instance.ModSorting);
        }

        private void SelectCurrentItem(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            item.IsSelected = true;
        }

        private void ImportModClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog importDialog = new OpenFileDialog();
            importDialog.Multiselect = true;
            importDialog.Filter = "Zip files (*.zip)|*.zip|Zenova Mod Pack (*.zmp)|*.zmp";
            if (importDialog.ShowDialog() == true)
            {
                foreach (string file in importDialog.FileNames)
                    ModManager.instance.ImportMod(file);
                ModsListBox.Items.Refresh();
                
            }
        }

        private void SortChanged(object sender, SelectionChangedEventArgs e)
        {
            SortModList(Preferences.instance.ModSorting);
        }

        private void ModSelected(object sender, MouseButtonEventArgs e)
        {

        }

        private void ModDetailsClick(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteModClick(object sender, RoutedEventArgs e)
        {
            ModManager.instance.RemoveMod((sender as FrameworkElement).DataContext as Mod);
            ModsListBox.Items.Refresh();
        }

        protected void SortModList(Mod.ModSortType sortType)
        {
            string sortTypeString = sortType == Mod.ModSortType.ByLatestSupported ? "LatestSupported" : "Name";
            ListSortDirection sortDirection = sortType == Mod.ModSortType.ByLatestSupported ? ListSortDirection.Descending : ListSortDirection.Ascending;
            if (ModsListBox != null)
            {
                ModsListBox.Items.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortTypeString, sortDirection);
                ModsListBox.Items.SortDescriptions.Add(sd);
            }
        }
    }
}
