using Microsoft.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallProducer
{
	class Program
	{
		static void Main(string[] args)
		{
			string eventHubName = "callstatsin";
			int numOfConnections = 2;
			int numOfLinksPerConnection = 100;

			for (int i = 0; i < numOfConnections; i++)
			{
				Task.Factory.StartNew((idx) =>
					{
						CallManager manager = new CallManager(numOfLinksPerConnection, eventHubName, "Connection" + idx);
						manager.StartMakingCalls();
					}, i);
			}

			Console.ReadLine();
		}
	}
}
