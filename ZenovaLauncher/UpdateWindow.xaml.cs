using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace ZenovaLauncher
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public List<ZenovaUpdater.AssemblyType> UpdateTypes { get; set; }

        public UpdateWindow(List<ZenovaUpdater.AssemblyType> updateTypes)
        {
            UpdateTypes = updateTypes;
            DataContext = ZenovaUpdater.instance;

            InitializeComponent();
        }

        private async void StartUpdate(object sender, RoutedEventArgs e)
        {
            ZenovaUpdater.instance.CancelCommand = new RelayCommand((o) => ZenovaUpdater.instance.cancelSource.Cancel());
            await Task.Run(async () =>
            {
                foreach (var update in UpdateTypes)
                    await ZenovaUpdater.instance.DoUpdate(update);
            }, ZenovaUpdater.instance.cancelSource.Token);
            Close();
            ((App)Application.Current).StartMainWindow();
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
