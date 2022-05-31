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
    public partial class ProfileDialog : ContentDialog
    {
        public ObservableCollection<Mod> LoadedMods { get; set; }

        protected ProfileDialog()
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
            AvailableModsBox.ItemsSource = ModManager.instance.ToList();
            LoadedMods = new ObservableCollection<Mod>();
            LoadedModsBox.ItemsSource = LoadedMods;

            ModOptionsExpander.Visibility = Visibility.Visible;

            if (VersionBox.SelectedItem != null && !(VersionBox.SelectedItem as MinecraftVersion).ModSupported)
            {
                ModOptionsExpander.Visibility = Visibility.Collapsed;
            }
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

        protected virtual void AddMod(Mod m)
        {
            LoadedMods.Add(m);
        }

        protected virtual void RemoveMod(Mod m)
        {
            LoadedMods.Remove(m);
        }

        private void AddModClick(object sender, RoutedEventArgs e)
        {
            AddMod((sender as FrameworkElement).DataContext as Mod);
            FilterModsList();
            RefreshMods();
        }

        private void RemoveModClick(object sender, RoutedEventArgs e)
        {
            RemoveMod((sender as FrameworkElement).DataContext as Mod);
            FilterModsList();
            RefreshMods();
        }
    }

    public partial class CreateProfileDialog : ProfileDialog
	{
        public CreateProfileDialog()
		{
            Title = "Create profile";
            PrimaryButtonText = "Create";
            PrimaryButtonClick += CreateProfile;

            VersionBox.SelectedIndex = 0;

            if (ModOptionsExpander.Visibility == Visibility.Visible)
                FilterModsList();
        }

        private void CreateProfile(object sender, ContentDialogButtonClickEventArgs e)
        {
            ProfileManager.instance.Add(new Profile(ProfileNameBox.Text, VersionBox.SelectedItem as MinecraftVersion, created: DateTime.Now, modsList: LoadedMods.ToList()));
        }
    }

    public partial class EditProfileDialog : ProfileDialog
    {
        public Profile EditedProfile { get; set; }
        public List<Mod> ModsToAdd = new List<Mod>();
        public List<Mod> ModsToRemove = new List<Mod>();

        public EditProfileDialog(Profile profile)
        {
            Title = "Edit profile";
            PrimaryButtonText = "Save";
            PrimaryButtonClick += SaveProfile;

            ProfileNameBox.Text = profile.Name;
            ProfileNameBox.IsEnabled = profile.Editable;
            VersionBox.SelectedItem = profile.Version;
            VersionBox.IsEnabled = profile.Editable;
            if (profile.ModsList != null)
                LoadedMods = new ObservableCollection<Mod>(profile.ModsList);
            // todo: can we update the existing binding?
            LoadedModsBox.ItemsSource = LoadedMods;

            if (!profile.Editable)
                ModOptionsExpander.Visibility = Visibility.Collapsed;

            if (ModOptionsExpander.Visibility == Visibility.Visible)
                FilterModsList();
        }

        protected override void AddMod(Mod m)
        {
            base.AddMod(m);
            if (ModsToRemove.Contains(m))
                ModsToRemove.Remove(m);
            else
                ModsToAdd.Add(m);
        }

        protected override void RemoveMod(Mod m)
        {
            if (ModsToAdd.Contains(m))
                ModsToAdd.Remove(m);
            else
                ModsToRemove.Add(m);
            base.RemoveMod(m);
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
