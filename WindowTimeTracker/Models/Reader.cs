using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WindowTimeTracker.Models
{

	/// <summary>
	/// the program class.
	/// </summary>
	public class Reader
	{
		/// <summary>
		/// Method wrapper for native methods called from c#.
		/// </summary>
		private class NativeMethods
		{
			/// <summary>
			/// Gets a window handle (HWND) as an IntPtr to the currently active foreground window.
			/// </summary>
			/// <returns>Returns the window handle.</returns>
			[DllImport("user32.dll")]
			public static extern IntPtr GetForegroundWindow();

			/// <summary>
			/// Retrieves the process id of process that the window belongs to.
			/// </summary>
			/// <param name="windowHandle">the window handle to retrieve the process id for.</param>
			/// <param name="processId">the process id (return value).</param>
			/// <returns>The thread id that created the window.</returns>
			[DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
			public static extern int GetWindowThreadProcessId(IntPtr windowHandle, out int processId);

			/// <summary>
			/// Gets the text of the window specified by the hWnd window handle.
			/// </summary>
			/// <param name="hWnd">the window handle used.</param>
			/// <param name="text">a <see cref="StringBuilder"/> used to store the text. At least <see cref="count"/> characters should be reserved in the string builder instance.</param>
			/// <param name="count">the buffer length.</param>
			/// <returns>returns the number of characters copied.</returns>
			[DllImport("user32.dll")]
			public static extern int GetWindowText(int hWnd, StringBuilder text, int count);
		}

		/// <summary>
		/// a reference to the last active window title.
		/// </summary>
		private string lastWindowTitle;

		/// <summary>
		/// writes the current window information to the console output. Only does so if the previous window title is different from the current window title.
		/// </summary>
		private void WriteCurrentWindowInformation()
		{
			var activeWindowId = NativeMethods.GetForegroundWindow();

			// no (valid) foreground window => no trackable data!
			if (activeWindowId.Equals(0))
			{
				return;
			}

			int processId;
			NativeMethods.GetWindowThreadProcessId(activeWindowId, out processId);

			// no (valid) process for window => no trackable data!
			if (processId == 0)
			{
				return;
			}

			Process foregroundProcess = Process.GetProcessById(processId);

			var fileName = string.Empty;
			var fileDescription = string.Empty;
			var productName = string.Empty;
			var processName = string.Empty;
			var windowTitle = string.Empty;

			try
			{
				if (!string.IsNullOrEmpty(foregroundProcess.ProcessName))
				{
					processName = foregroundProcess.ProcessName;
				}
			}
			catch (Exception)
			{
			}

			try
			{
				if (!string.IsNullOrEmpty(foregroundProcess.MainModule.FileName))
				{
					fileName = foregroundProcess.MainModule.FileName;
				}
			}
			catch (Exception)
			{
			}

			try
			{
				if (!string.IsNullOrEmpty(foregroundProcess.MainModule.FileVersionInfo.FileDescription))
				{
					fileDescription = foregroundProcess.MainModule.FileVersionInfo.FileDescription;
				}
			}
			catch (Exception)
			{
			}

			try
			{
				if (!string.IsNullOrEmpty(foregroundProcess.MainModule.FileVersionInfo.ProductName))
				{
					productName = foregroundProcess.MainModule.FileVersionInfo.ProductName;
				}
			}
			catch (Exception)
			{
			}

			try
			{
				if (!string.IsNullOrEmpty(foregroundProcess.MainWindowTitle))
				{
					windowTitle = foregroundProcess.MainWindowTitle;
				}
			}
			catch (Exception)
			{
			}

			try
			{
				if (string.IsNullOrEmpty(windowTitle))
				{
					const int Count = 1024;
					var sb = new StringBuilder(Count);
					NativeMethods.GetWindowText((int)activeWindowId, sb, Count);

					windowTitle = sb.ToString();
				}
			}
			catch (Exception)
			{
			}

			if (lastWindowTitle != windowTitle)
			{
				//Console.WriteLine("ProcessId: {0}\nFilename: {1}\nFileDescription: {2}\nProductName: {3}\nProcessName: {4}\nWindowTitle: {5}\nWindowHandle: {6}\n",
				//	Convert.ToString(processId),
				//	fileName,
				//	fileDescription,
				//	productName,
				//	processName,
				//	windowTitle,
				//	Convert.ToString(activeWindowId));

				Application.Current?.Dispatcher.Invoke(() =>
				{
					Configurations.Instance.Log.Add(new WindowInformations()
					{
						ForegroundProcess = foregroundProcess,
						FileName = fileName,
						FileDescription = fileDescription,
						ProductName = productName,
						ProcessName = processName,
						WindowTitle = windowTitle
					});
                    Configurations.Instance.StringLog += $"{DateTime.Now.ToUniversalTime()},{fileDescription},{productName},{processName},{windowTitle}\n";
                });

				lastWindowTitle = windowTitle;
			}
		}
		Point _originalMousePos;
		LowLevelKeyboardListener _listener = null;
		void HandleInactivity()
		{ 
			Application.Current.Dispatcher.Invoke(() =>
			{
				#region Keyboard detection
				void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
				{
					//works on ui thread only
					Configurations.Instance.InactivityCount = 0;
				}
				if(_listener == null)
				{
					_listener = new LowLevelKeyboardListener();
					_listener.OnKeyPressed -= _listener_OnKeyPressed;
					_listener.OnKeyPressed += _listener_OnKeyPressed;
					_listener.HookKeyboard();
				}
			
				if (Application.Current == null)
				{
					_listener.UnHookKeyboard();
				}
				#endregion
				#region MouseActivity
				Point _currentMousePos = MousePosition.GetCursorPosition();
				if (_currentMousePos != _originalMousePos)
				{
					//works on ui thread
					Configurations.Instance.InactivityCount = 0;
					_originalMousePos = _currentMousePos;
				}
				#endregion
				if (Configurations.Instance.InactivityCount > Configurations.Instance.InactivityTrigger)
				{
					Configurations.Instance.IsTracking = false;
					//if (MessageBox.Show("Automatically deactivated log due to inactivity. Reactivate?", "Deactivated", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
					//{
					//	Configurations.Instance.IsTracking = true;
					//}
				}
				else
				{
					Configurations.Instance.IsTracking = true;
				}
				Configurations.Instance.InactivityCount += 1;
			});
		}

		public Reader()
		{

			Task.Run(() =>
			{
                while(Application.Current != null)
                {
                    if (Configurations.Instance.IsTracking)
                    {
						WriteCurrentWindowInformation();
					}
					if (Configurations.Instance.InactivityTrigger != 0)
					{
						//auto activate/deactivate if motion detected
						HandleInactivity();
					}
					Thread.Sleep(Configurations.Instance.ScanIntervalS * 1000);
				}
			});

		}


        public void StopTracking()
        {
			throw new NotImplementedException();
        }
	}
}
