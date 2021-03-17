using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WindowTimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        static Views.MainWindow _guiWindow = null;
        public static Views.MainWindow GuiWindow
        {
            get
            {
                if (_guiWindow == null)
                {
                    _guiWindow = new Views.MainWindow();
                }
                return _guiWindow;
            }
        }

        /// <summary>
        /// keeps sync of present Dialog Host instances, popping is handled by Dialog Hosts (unloaded event)
        /// </summary>
        public App()
        {
            Application.Current.DispatcherUnhandledException += (s, a) =>
            {
                MessageBox.Show(a.Exception.ToString());
                // Prevent default unhandled exception processing
                a.Handled = false;
            };

        }



        bool _isMaximizingDoubleClick = false;
        double _normalWindowWidth = 0;
        double _normalWindowHeight = 0;

        private void ColorZone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_isMaximizingDoubleClick)
                {
                    _isMaximizingDoubleClick = false;
                }
                else if (GuiWindow.WindowState == WindowState.Maximized)
                {
                    GuiWindow.Left = GetCursorPosition().X - (_normalWindowWidth * Mouse.GetPosition(GuiWindow).X / SystemParameters.WorkArea.Width);
                    GuiWindow.Top = GetCursorPosition().Y - (_normalWindowHeight * Mouse.GetPosition(GuiWindow).Y / SystemParameters.WorkArea.Height);
                    GuiWindow.WindowState = WindowState.Normal;
                }
                else if (GuiWindow.WindowState == WindowState.Normal)
                {
                    _normalWindowHeight = GuiWindow.Height;
                    _normalWindowWidth = GuiWindow.Width;
                }
                GuiWindow.DragMove();
            }
            //this.MaxHeight = SystemParameters.WorkArea.Height;
            //this.MaxWidth = SystemParameters.WorkArea.Width;
        }
        private void ColorZone_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GuiWindow.WindowState == WindowState.Normal)
            {
                //without waiting the drag event gets called after maximizing
                GuiWindow.WindowState = WindowState.Maximized;
                _isMaximizingDoubleClick = true;
                return;
            }
            else
            {
                GuiWindow.WindowState = WindowState.Normal;
                return;
            }
        }
        //https://stackoverflow.com/questions/1316681/getting-mouse-position-in-c-sharp
        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            //bool success = User32.GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }

    }
}
