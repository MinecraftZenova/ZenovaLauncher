using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for ProfilesPage.xaml
    /// </summary>
    public partial class ProfilesPage : Page
    {
        public ProfilesPage()
        {
            InitializeComponent();

            DataContext = ProfileLauncher.instance;
            ProfileListBox.ItemsSource = ProfileManager.instance;
            SortProfileBox.DataContext = Preferences.instance;
            ReleasesBox.DataContext = Preferences.instance;
            BetasBox.DataContext = Preferences.instance;
            HistoricalBox.DataContext = Preferences.instance;

            ProfileManager.instance.Refresh = RefreshProfiles;

            SortProfileList(Preferences.instance.ProfileSorting);
            FilterProfileList();
        }

        private void RefreshProfiles()
        {
            ProfileListBox.Items.Refresh();
        }

        private void SelectCurrentItem(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            item.IsSelected = true;
        }

        private async void AddProfileClick(object sender, RoutedEventArgs e)
        {
            AddProfileDialog newProfile = new AddProfileDialog();
            var result = await newProfile.ShowAsync();
            if (result == ModernWpf.Controls.ContentDialogResult.Primary)
                RefreshProfiles();
        }

        private void EditProfileClick(object sender, RoutedEventArgs e)
        {
            EditProfile((sender as FrameworkElement).DataContext as Profile);
        }

        private void DuplicateProfileClick(object sender, RoutedEventArgs e)
        {
            Profile newProfile = new Profile((sender as FrameworkElement).DataContext as Profile);
            int index = 2;
            while (ProfileManager.instance.SingleOrDefault(p => p.Name == (newProfile.Name + " (" + index + ")")) != null)
                index++;
            newProfile.Name += " (" + index + ")";
            ProfileManager.instance.Add(newProfile);
            SortProfileList(Preferences.instance.ProfileSorting);
        }

        private void DeleteProfileClick(object sender, RoutedEventArgs e)
        {
            ProfileManager.instance.Remove((sender as FrameworkElement).DataContext as Profile);
            VersionManager.instance.RemoveUnusedVersions();
            RefreshProfiles();
        }

        private void ProfileSelected(object sender, MouseButtonEventArgs e)
        {
            EditProfile((sender as FrameworkElement).DataContext as Profile);
        }

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            ProfileManager.instance.SelectedProfile.SelectedProfile = (sender as FrameworkElement).DataContext as Profile;
            ProfileLauncher.instance.LaunchProfile((sender as FrameworkElement).DataContext as Profile);
        }

        private void SortChanged(object sender, SelectionChangedEventArgs e)
        {
            SortProfileList(Preferences.instance.ProfileSorting);
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
            FilterProfileList();
        }

        protected void FilterProfileList()
        {
            List<Predicate<object>> predicates = new List<Predicate<object>>();
            if (Preferences.instance.EnableReleases)
                predicates.Add(Profile.releaseFilter);
            if (Preferences.instance.EnableBetas)
                predicates.Add(Profile.betaFilter);
            if (Preferences.instance.EnableHistorical)
                predicates.Add(Profile.historicalFilter);
            if (ProfileListBox != null)
                ProfileListBox.Items.Filter = o => predicates.Any(predicate => predicate(o));
        }

        protected void SortProfileList(Profile.ProfileSortType sortType)
        {
            string sortTypeString = sortType == Profile.ProfileSortType.ByLastPlayed ? "LastUsed" : "Name";
            ListSortDirection sortDirection = sortType == Profile.ProfileSortType.ByLastPlayed ? ListSortDirection.Descending : ListSortDirection.Ascending;
            if (ProfileListBox != null)
            {
                ProfileListBox.Items.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortTypeString, sortDirection);
                ProfileListBox.Items.SortDescriptions.Add(sd);
            }
        }

        protected async void EditProfile(Profile profile)
        {
            EditProfileDialog editProfile = new EditProfileDialog(profile);
            var result = await editProfile.ShowAsync();
            if (result == ModernWpf.Controls.ContentDialogResult.Primary)
                RefreshProfiles();
        }
    }
}
