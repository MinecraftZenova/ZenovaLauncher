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
using ModernWpf.Controls;

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
            ProfileNameBox.Text = profile.ProfileName;
            VersionBox.ItemsSource = VersionManager.instance;
            VersionBox.Items.SortDescriptions.Clear();
            VersionBox.Items.SortDescriptions.Add(new SortDescription("SortOrder", ListSortDirection.Ascending));
            VersionBox.Items.SortDescriptions.Add(new SortDescription("Version", ListSortDirection.Descending));
            VersionBox.SelectedItem = profile.Version;
        }

        public Profile EditedProfile { get; set; }
    }
}
