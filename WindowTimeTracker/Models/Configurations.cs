using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WindowTimeTracker.Models
{
    class Configurations : BaseClass
    {
        private static Configurations _instance = new Configurations();
        public static Configurations Instance
        {
            get
            {
                //if(_instance == null)
                //{
                //    _instance = new Configurations();
                //}
                return _instance;
            }
        }
        Reader _reader;

        private Configurations()
        {
            _reader = new Reader();
            IsTracking = true;
            //Log.CollectionChanged += Log_CollectionChanged;
        }

        //private void Log_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        //    {
        //        foreach (WindowInformations _newItem in e.NewItems)
        //        {
        //            Application.Current.Dispatcher.Invoke(() =>
        //            {

        //                StringLog += $"{DateTime.Now.ToUniversalTime()},{_newItem.FileDescription},{_newItem.ProductName},{_newItem.ProcessName},{_newItem.WindowTitle}\n";
        //            });
        //        }
        //    }
        //}

        bool _isTracking = false;
        public bool IsTracking
        {
            get => _isTracking;
            set
            {
                if(value != _isTracking)
                {
                    _isTracking = value;
                }
                OnPropertyChanged();
            }
        }
        string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WindowLog.log");
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<WindowInformations> _log = new ObservableCollection<WindowInformations>();
        public ObservableCollection<WindowInformations> Log
        {
            get => _log;
            set
            {
                _log = value;
                OnPropertyChanged();
            }
        }
        string _stringLog;
        public string StringLog
        {
            get => _stringLog;
            set
            {
                _stringLog = value;
                OnPropertyChanged();
            }
        }
        int _scanIntervalS = 10;
        public int ScanIntervalS
        {
            get => _scanIntervalS;
            set
            {
                _scanIntervalS = value;
                OnPropertyChanged();
            }
        }
        public bool SaveLogFile()
        {
            try
            {
                var _logFile = new FileInfo(_filePath);
                if (!_logFile.Exists)
                {
                    _logFile.Create();
                }
                var _logStream = _logFile.AppendText();
                //write header
                if (_logFile.Length == 0)
                {
                    _logStream.WriteLine("DateTime,FileDescription,ProductName,ProcessName,WindowTitle");
                }
                _logStream.Write(_stringLog);
                _logStream.Close();
                StringLog = "";
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}
