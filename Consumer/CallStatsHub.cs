using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer
{
	public class CallStatsHub : Hub
	{
		public void UpdateClients(int started, int stopped, int inprogress)
		{
			Clients.All.update(started, stopped, inprogress);
		}
	}
}
