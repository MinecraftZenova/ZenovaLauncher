using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for AddProfileDialog.xaml
    /// </summary>
    public partial class EditProfileDialog : ContentDialog
    {
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
        }

        private void SaveProfile(object sender, ContentDialogButtonClickEventArgs e)
        {
            EditedProfile.Name = ProfileNameBox.Text;
            EditedProfile.Version = VersionBox.SelectedItem as MinecraftVersion;
        }

        public Profile EditedProfile { get; set; }
    }
}
