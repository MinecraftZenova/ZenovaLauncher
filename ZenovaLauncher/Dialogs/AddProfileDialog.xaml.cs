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
    public partial class AddProfileDialog : ContentDialog
    {
        public AddProfileDialog(bool showReleases, bool showBetas, bool showHistorical)
        {
            InitializeComponent();
            VersionBox.ItemsSource = VersionManager.instance;
            VersionBox.Items.SortDescriptions.Clear();
            VersionBox.Items.SortDescriptions.Add(new SortDescription("SortOrder", ListSortDirection.Ascending));
            VersionBox.Items.SortDescriptions.Add(new SortDescription("Version", ListSortDirection.Descending));
            List<Predicate<object>> predicates = new List<Predicate<object>>();
            if (showReleases)
                predicates.Add(MinecraftVersion.releaseFilter);
            if (showBetas)
                predicates.Add(MinecraftVersion.betaFilter);
            if (showHistorical)
                predicates.Add(MinecraftVersion.historicalFilter);
            VersionBox.Items.Filter = o => predicates.Any(predicate => predicate(o));
            VersionBox.SelectedIndex = 0;
        }

        private void CreateProfile(object sender, ContentDialogButtonClickEventArgs e)
        {
            ProfileManager.instance.Add(new Profile(ProfileNameBox.Text, VersionBox.SelectedItem as MinecraftVersion));
        }
    }
}
