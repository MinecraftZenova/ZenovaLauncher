using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for AddProfileDialog.xaml
    /// </summary>
    public partial class AddProfileDialog : ContentDialog
    {
        public ObservableCollection<Mod> LoadedMods;

        public AddProfileDialog()
        {
            InitializeComponent();
            VersionBox.ItemsSource = VersionManager.instance;
            VersionBox.Items.SortDescriptions.Clear();
            VersionBox.Items.SortDescriptions.Add(new SortDescription("SortOrder", ListSortDirection.Ascending));
            VersionBox.Items.SortDescriptions.Add(new SortDescription("Version", ListSortDirection.Descending));
            List<Predicate<object>> predicates = new List<Predicate<object>>();
            if (Preferences.instance.EnableReleases)
                predicates.Add(MinecraftVersion.releaseFilter);
            if (Preferences.instance.EnableBetas)
                predicates.Add(MinecraftVersion.betaFilter);
            if (Preferences.instance.EnableHistorical)
                predicates.Add(MinecraftVersion.historicalFilter);
            VersionBox.Items.Filter = o => predicates.Any(predicate => predicate(o));
            VersionBox.SelectedIndex = 0;
            AvailableModsBox.ItemsSource = ModManager.instance.ToList();
            LoadedMods = new ObservableCollection<Mod>();
            LoadedModsBox.ItemsSource = LoadedMods;
            FilterModsList();
        }

        public void RefreshMods()
        {
            AvailableModsBox.Items.Refresh();
            LoadedModsBox.Items.Refresh();
        }

        private void VersionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveUnsupportedMods();
            FilterModsList();
            RefreshMods();
        }

        private void AddModClick(object sender, RoutedEventArgs e)
        {
            LoadedMods.Add((sender as FrameworkElement).DataContext as Mod);
            FilterModsList();
            RefreshMods();
        }

        private void RemoveModClick(object sender, RoutedEventArgs e)
        {
            LoadedMods.Remove((sender as FrameworkElement).DataContext as Mod);
            FilterModsList();
            RefreshMods();
        }

        protected void RemoveUnsupportedMods()
        {
            if (LoadedMods != null)
            {
                foreach (Mod m in LoadedMods.Where(m => !m.SupportsVersion(VersionBox.SelectedItem as MinecraftVersion)).ToList())
                    LoadedMods.Remove(m);
            }
        }

        protected void FilterModsList()
        {
            if (AvailableModsBox != null && LoadedMods != null)
            {
                List<Predicate<object>> predicates = new List<Predicate<object>>
                {
                    m => (m as Mod).SupportsVersion(VersionBox.SelectedItem as MinecraftVersion),
                    m => !LoadedMods.Contains(m as Mod)
                };
                AvailableModsBox.Items.Filter = o => predicates.All(predicate => predicate(o));
            }
        }

        private void CreateProfile(object sender, ContentDialogButtonClickEventArgs e)
        {
            ProfileManager.instance.Add(new Profile(ProfileNameBox.Text, VersionBox.SelectedItem as MinecraftVersion, created: DateTime.Now, modsList: LoadedMods.ToList()));
        }
    }
}
