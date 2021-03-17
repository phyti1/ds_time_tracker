using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WindowTimeTracker.Models;

namespace WindowTimeTracker.ViewModels
{
    class MainWindowViewModel : BaseClass
    {
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
        public ICommand SaveLogCommand { get; }
        public MainWindowViewModel()
        {
            SaveLogCommand = new RelayCommand((arg) =>
            {
                Configurations.Instance.SaveLogFile(false);
            });
            Task.Run(() =>
            {
                while (Application.Current != null)
                {
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        StringLog = Configurations.Instance.StringLog;
                    });
                    Thread.Sleep(1000);
                }
            });
        }
    }
}
