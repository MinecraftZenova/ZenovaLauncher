using ModernWpf.Controls;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for DeleteConfirmationDialog.xaml
    /// </summary>
    public partial class ErrorDialog : ContentDialog
    {
        public ErrorDialog(string errorTitle, string errorMessage)
        {
            InitializeComponent();

            ErrorTitle.Text = errorTitle;
            ErrorMessage.Text = errorMessage;
        }
    }
}
