using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WindowTimeTracker.Models
{
    class Configurations : BaseClass
    {
        private static Configurations _instance = null;
        public static Configurations Instance
        {
            get
            {
                if (_instance == null)
                {
                    Configurations.Deserialize(_configFilePath, true);
                }
                return _instance;
            }
            private set
            {
                _instance = value;
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
        private string GetOffLine()
        {
            return $"{DateTime.Now.ToUniversalTime()},OFF,,,\n";
        }
        bool _isTracking = false;
        [JsonIgnore]
        public bool IsTracking
        {
            get => _isTracking;
            set
            {
                if(value != _isTracking)
                {
                    if(value == false)
                    {
                        //add empty line when deactivating
                        StringLog += GetOffLine();
                    }
                    _isTracking = value;
                }
                OnPropertyChanged();
            }
        }
        string _logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WindowLog.log");
        public string LogFilePath
        {
            get => _logFilePath;
            set
            {
                _logFilePath = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<WindowInformations> _log = new ObservableCollection<WindowInformations>();
        [JsonIgnore]
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
        [JsonIgnore]
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
        int _inactivityCount = 0;
        [JsonIgnore]
        public int InactivityCount
        {
            get => _inactivityCount;
            set
            {
                _inactivityCount = value;
                OnPropertyChanged();
            }
        }
        int _inactivityTrigger = 40;
        //if trigger=0 -> decativated
        public int InactivityTrigger
        {
            get => _inactivityTrigger;
            set
            {
                _inactivityTrigger = value;
                OnPropertyChanged();
            }
        }
        public bool SaveLogFile(bool Ab_Closing)
        {
            try
            {
                var _logFile = new FileInfo(_logFilePath);
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
                if (Ab_Closing)
                {
                    _logStream.Write(GetOffLine());
                }
                _logStream.Close();
                StringLog = "";
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        [JsonIgnore]
        internal static bool _configIsLoading = false;
        private static string _configFilePath
        {
            get
            {
                var _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WindowTracker", "WindowTracker.json");
                if (!Directory.Exists(Path.GetDirectoryName(_configPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_configPath));
                }
                if (!File.Exists(_configPath))
                {
                    File.Create(_configPath).Close();
                }
                return _configPath;
            }
        }

        private static object _lockobj = new object();
        public static void TrySerialize()
        {
            if (_instance == null || string.IsNullOrWhiteSpace(_configFilePath)) { throw new Exception("Cannot serialize"); }
            lock (_lockobj)
            {
                try
                {
                    if (File.Exists(_configFilePath) == false)
                    {
                        File.Create(_configFilePath).Close();
                    }
                    //otherwise it would add it
                    File.WriteAllText(_configFilePath, string.Empty);
                    using (var streamWriter = new StreamWriter(_configFilePath, true))
                    {
                        var _jsonString = JsonConvert.SerializeObject(Instance);
                        streamWriter.WriteLine(_jsonString);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Saving configurations failed <{ex.Message}>", "Error", MessageBoxButton.OK);
                }
            }
        }
        private static Exception Deserialize(string path, bool overrideExisting)
        {
            lock (_lockobj)
            {
                Configurations.Instance = null;
                _configIsLoading = true;
                try
                {
                    var _fileContent = File.ReadAllText(path);
                    Instance = JsonConvert.DeserializeObject<Configurations>(_fileContent);
                    if (_instance == null)
                    {
                        throw new Exception("deserialize returned null");
                    }
                    _configIsLoading = false;
                    //Instance.Initialize();
                    return null;
                }
                catch (Exception ex)
                {
                    //if file was not encrypted before
                    try
                    {
                        var _fileContent = File.ReadAllText(path);
                        Instance = JsonConvert.DeserializeObject<Configurations>(_fileContent);
                        if (_instance == null)
                        {
                            throw new Exception("deserialize returned null");
                        }
                        _configIsLoading = false;
                        //Instance.Initialize();
                        return null;
                    }
                    catch (Exception ex2) { Debug.WriteLine(ex2.Message); }
                    //occurs when application is opened for the first time
                    if (overrideExisting)
                    {
                        Configurations.Instance = new Configurations();
                        _configIsLoading = false;
                        TrySerialize();
                    }
                    _configIsLoading = false;
                    //Instance.Initialize();
                    return ex;
                }
            }
        }
    }
}
