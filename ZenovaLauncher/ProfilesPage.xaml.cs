using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for ProfilesPage.xaml
    /// </summary>
    public partial class ProfilesPage : Page
    {
        Predicate<object> releaseFilter = (object item) =>
        {
            return (item as Profile).Release == true;
        };
        Predicate<object> betaFilter = (object item) =>
        {
            return (item as Profile).Beta == true;
        };
        Predicate<object> historicalFilter = (object item) =>
        {
            return (item as Profile).Historical == true;
        };

        public ProfilesPage() {
            InitializeComponent();

            SortProfileList(true);
            FilterProfileList();
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
        }

        private void SortChanged(object sender, SelectionChangedEventArgs e)
        {
            SortProfileList((sender as ComboBox).SelectedIndex == 0);
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
            FilterProfileList();
        }

        protected void FilterProfileList()
        {
            List<Predicate<object>> predicates = new List<Predicate<object>>();
            if (ReleasesBox.IsChecked == true)
                predicates.Add(releaseFilter);
            if (BetasBox.IsChecked == true)
                predicates.Add(betaFilter);
            if (HistoricalBox.IsChecked == true)
                predicates.Add(historicalFilter);
            if(ProfileListBox != null)
                ProfileListBox.Items.Filter = o => predicates.Any(predicate => predicate(o));
        }

        protected void SortProfileList(bool sortType)
        {
            string sortTypeString = sortType ? "LastPlayed" : "ProfileName";
            ListSortDirection sortDirection = sortType ? ListSortDirection.Descending : ListSortDirection.Ascending;
            if (ProfileListBox != null)
            {
                ProfileListBox.Items.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortTypeString, sortDirection);
                ProfileListBox.Items.SortDescriptions.Add(sd);
            }
        }
    }
}
