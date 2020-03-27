﻿using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            NavView.SelectedItem = NavViewItems.OfType<HamburgerMenuItem>().First();
            Navigate(NavView.SelectedItem);
        }

        private void NavView_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            if (e.IsItemOptions)
            {
                Navigate(NavView.SelectedOptionsItem);
            }
            else
            {
                Navigate(NavView.SelectedItem);
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.SelectedItem = NavViewItems.OfType<HamburgerMenuItem>().FirstOrDefault(x => GetNavigateUri(x) == e.Uri);
            NavView.SelectedOptionsItem = NavViewOptions.OfType<HamburgerMenuItem>().FirstOrDefault(x => GetNavigateUri(x) == e.Uri);

            var selectedItem = NavView.SelectedItem ?? NavView.SelectedOptionsItem;
            if (selectedItem is HamburgerMenuItem item)
            {
                NavView.Header = item.Label;
            }
        }

        private void Navigate(object item)
        {
            if (item is HamburgerMenuItem menuItem)
            {
                Uri navigateUri = GetNavigateUri(menuItem);
                if (ContentFrame.CurrentSource != navigateUri)
                {
                    ContentFrame.Navigate(navigateUri);
                }
            }
        }

        private Uri GetNavigateUri(HamburgerMenuItemBase item)
        {
            if (item.Tag is Uri uri)
            {
                return uri;
            }
            return new Uri((string)item.Tag, UriKind.Relative);
        }
    }

}
