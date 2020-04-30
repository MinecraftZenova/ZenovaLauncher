using ModernWpf.Controls;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for DeleteConfirmationDialog.xaml
    /// </summary>
    public partial class DeleteConfirmationDialog : ContentDialog
    {
        public DeleteConfirmationDialog(string nameToDelete)
        {
            InitializeComponent();

            DeleteItemName.Text = nameToDelete;
        }
    }
}
