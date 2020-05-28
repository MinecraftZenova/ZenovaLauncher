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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
        {
            DataContext = ZenovaUpdater.instance;

            InitializeComponent();
        }

        private async void StartUpdate(object sender, RoutedEventArgs e)
        {
            ZenovaUpdater.instance.CancelCommand = new RelayCommand((o) => ZenovaUpdater.instance.cancelSource.Cancel());
            await Task.Run(async () =>
            {
                bool success = await ZenovaUpdater.instance.DoUpdate();
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate ()
                {
                    if (success)
                        Application.Current.Shutdown();
                    else
                        ((App)Application.Current).StartMainWindow();
                });
            }, ZenovaUpdater.instance.cancelSource.Token);
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            StopUpdate();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            StopUpdate();
        }

        private void StopUpdate()
        {
            ZenovaUpdater.instance.CancelCommand.Execute(null);
        }
    }
}
