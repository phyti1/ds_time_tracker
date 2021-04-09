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
using WindowTimeTracker.ViewModels;

namespace WindowTimeTracker.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel o_ViewModel;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = o_ViewModel = new MainWindowViewModel();
            Closing += MainWindow_Closing;
            //Closed += MainWindow_Closed;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Models.Configurations.TrySerialize();
#if DEBUG
            var _result = MessageBox.Show("Save config & data to log?", "Warning", MessageBoxButton.YesNoCancel);
            if (_result == MessageBoxResult.Yes)
            {
                Models.Configurations.Instance.SaveLogFile(true);
                Models.Configurations.TrySerialize();

            }
            else if(_result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
#else
            //always save in release
            Models.Configurations.Instance.SaveLogFile(true);
            Models.Configurations.TrySerialize();
#endif

        }

        //private void MainWindow_Closed(object sender, EventArgs e)
        //{
        //}

    }
}
