﻿using Microsoft.Win32;
using ModernWpf.Controls;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for ModsPage.xaml
    /// </summary>
    public partial class ModsPage : System.Windows.Controls.Page
    {
        public ModsPage()
        {
            InitializeComponent();

            DataContext = ProfileLauncher.instance;
            ModsListBox.ItemsSource = ModManager.instance;
            SortModsBox.DataContext = Preferences.instance;

            ModManager.instance.Refresh = RefreshMods;

            SortModList(Preferences.instance.ModSorting);
        }

        private void RefreshMods()
        {
            ModsListBox.Items.Refresh();
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
            importDialog.Filter = "Zenova Mod Package (*.zmp;*.zip)|*.zmp;*.zip";
            if (importDialog.ShowDialog() == true)
                ModManager.instance.TryImportMods(importDialog.FileNames.ToList());
        }

        private void SortChanged(object sender, SelectionChangedEventArgs e)
        {
            SortModList(Preferences.instance.ModSorting);
        }

        private void ModSelected(object sender, MouseButtonEventArgs e)
        {
            ModDetails((sender as FrameworkElement).DataContext as Mod);
        }

        private void ModDetailsClick(object sender, RoutedEventArgs e)
        {
            ModDetails((sender as FrameworkElement).DataContext as Mod);
        }

        private async void DeleteModClick(object sender, RoutedEventArgs e)
        {
            DeleteConfirmationDialog deleteMod = new DeleteConfirmationDialog(((sender as FrameworkElement).DataContext as Mod).Name);
            var result = await deleteMod.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ModManager.instance.RemoveMod((sender as FrameworkElement).DataContext as Mod);
                RefreshMods();
            }
        }

        protected void SortModList(Mod.ModSortType sortType)
        {
            string sortTypeString = sortType == Mod.ModSortType.ByLatestSupported ? "LatestSupportedVersion" : "Name";
            ListSortDirection sortDirection = sortType == Mod.ModSortType.ByLatestSupported ? ListSortDirection.Descending : ListSortDirection.Ascending;
            if (ModsListBox != null)
            {
                ModsListBox.Items.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortTypeString, sortDirection);
                ModsListBox.Items.SortDescriptions.Add(sd);
            }
        }

        protected async void ModDetails(Mod mod)
        {
            ModDetailsDialog modDetails = new ModDetailsDialog(mod);
            await modDetails.ShowAsync();
        }
    }
}
