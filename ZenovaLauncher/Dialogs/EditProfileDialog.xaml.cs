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
    public partial class EditProfileDialog : ContentDialog
    {
        public Profile EditedProfile { get; set; }
        public ObservableCollection<Mod> LoadedMods { get; set; }

        public EditProfileDialog(Profile profile)
        {
            InitializeComponent();
            EditedProfile = profile;
            ProfileNameBox.Text = profile.Name;
            ProfileNameBox.IsEnabled = profile.Editable;
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
            VersionBox.SelectedItem = profile.Version;
            VersionBox.IsEnabled = profile.Editable;
            AvailableModsBox.ItemsSource = ModManager.instance.ToList();
            LoadedMods = profile.ModsList != null ? new ObservableCollection<Mod>(profile.ModsList) : new ObservableCollection<Mod>();
            LoadedModsBox.ItemsSource = LoadedMods;
            FilterModsList();
            ModOptionsExpander.IsEnabled = profile.Editable;
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

        private void SaveProfile(object sender, ContentDialogButtonClickEventArgs e)
        {
            EditedProfile.Name = ProfileNameBox.Text;
            EditedProfile.Version = VersionBox.SelectedItem as MinecraftVersion;
            EditedProfile.ModsList = LoadedMods.Count > 0 ? LoadedMods : null;
        }
    }
}
