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
        public List<Mod> ModsToAdd = new List<Mod>();
        public List<Mod> ModsToRemove = new List<Mod>();

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

            ModOptionsExpander.Visibility = Visibility.Collapsed;

            if (VersionBox.SelectedItem != null && (VersionBox.SelectedItem as MinecraftVersion).ModSupported)
            {
                FilterModsList();

                if (profile.Editable)
                    ModOptionsExpander.Visibility = Visibility.Visible;
            }
        }

        public void RefreshMods()
        {
            AvailableModsBox.Items.Refresh();
            LoadedModsBox.Items.Refresh();
        }

        private void VersionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(VersionBox.SelectedItem as MinecraftVersion).ModSupported)
            {
                ModOptionsExpander.Visibility = Visibility.Collapsed;
                return;
            }

            ModOptionsExpander.Visibility = Visibility.Visible;

            RemoveUnsupportedMods();
            FilterModsList();
            RefreshMods();
        }

        private void AddModClick(object sender, RoutedEventArgs e)
        {
            Mod m = (sender as FrameworkElement).DataContext as Mod;
            LoadedMods.Add(m);
            if (ModsToRemove.Contains(m))
                ModsToRemove.Remove(m);
            else
                ModsToAdd.Add(m);
            FilterModsList();
            RefreshMods();
        }

        private void RemoveModClick(object sender, RoutedEventArgs e)
        {
            Mod m = (sender as FrameworkElement).DataContext as Mod;
            if (ModsToAdd.Contains(m))
                ModsToAdd.Remove(m);
            else
                ModsToRemove.Add(m);
            LoadedMods.Remove(m);
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
            ModsToRemove.ForEach(m => EditedProfile.RemoveMod(m));
            ModsToAdd.ForEach(m => EditedProfile.AddMod(m));
        }
    }
}
