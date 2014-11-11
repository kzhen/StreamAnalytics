using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Consumer.Models
{
	public class CallStats
	{
		public DateTime WinStartTime { get; set; }
		public DateTime WinEndTime { get; set; }
		public int Started { get; set; }
		public int Ended { get; set; }
		public int Dropped { get; set; }
		public int NotAnsweredBusy { get; set; }
		public string ConnectionName { get; set; }
	}
}
