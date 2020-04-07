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
            VersionBox.Text = profile.VersionName;
        }

        public Profile EditedProfile { get; set; }
    }
}
