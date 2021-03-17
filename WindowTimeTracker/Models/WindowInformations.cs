using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowTimeTracker.Models
{
    class WindowInformations
    {
		public Process ForegroundProcess;
		public DateTime UtcTimeStamp = DateTime.Now.ToUniversalTime();
		public string FileName { get; set; } = string.Empty;
		public string FileDescription { get; set; } = string.Empty;
		public string ProductName { get; set; } = string.Empty;
		public string ProcessName { get; set; } = string.Empty;
		public string WindowTitle { get; set; } = string.Empty;
	}
}
