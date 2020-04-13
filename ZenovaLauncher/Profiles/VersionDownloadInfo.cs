using System.Windows.Input;

namespace ZenovaLauncher
{
    public class VersionDownloadInfo : NotifyPropertyChangedBase
    {

        private bool _isInitializing;
        private bool _isExtracting;
        private long _downloadedBytes;
        private long _totalSize;

        public bool IsInitializing
        {
            get { return _isInitializing; }
            set { _isInitializing = value; OnPropertyChanged("IsProgressIndeterminate"); OnPropertyChanged("DisplayStatus"); }
        }

        public bool IsExtracting
        {
            get { return _isExtracting; }
            set { _isExtracting = value; OnPropertyChanged("IsProgressIndeterminate"); OnPropertyChanged("DisplayStatus"); }
        }

        public bool IsProgressIndeterminate
        {
            get { return IsInitializing || IsExtracting; }
        }

        public long DownloadedBytes
        {
            get { return _downloadedBytes; }
            set { _downloadedBytes = value; OnPropertyChanged("DownloadedBytes"); OnPropertyChanged("DisplayStatus"); }
        }

        public long TotalSize
        {
            get { return _totalSize; }
            set { _totalSize = value; OnPropertyChanged("TotalSize"); OnPropertyChanged("DisplayStatus"); }
        }

        public string DisplayStatus
        {
            get
            {
                if (IsInitializing)
                    return "Downloading...";
                if (IsExtracting)
                    return "Extracting...";
                return "Downloading... " + (DownloadedBytes / 1024 / 1024) + "MiB/" + (TotalSize / 1024 / 1024) + "MiB";
            }
        }

        public ICommand CancelCommand { get; set; }
    }
}
