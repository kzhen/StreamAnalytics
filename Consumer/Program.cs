using Consumer.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Consumer
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Starting...");

			string url = "http://localhost:8080";

			var webApp = WebApp.Start<StartUp>(url);
			
			Console.WriteLine("Server running on: " + url);
			
			IHubContext hub = GlobalHost.ConnectionManager.GetHubContext<CallStatsHub>();

			var cts = new CancellationTokenSource();

			for (int i = 0; i < 8; i++)
			{
				Task.Factory.StartNew((state) =>
					{
						Console.WriteLine("Starting worker to process partition: {0}", state);

						var factory = MessagingFactory.Create(ServiceBusEnvironment.CreateServiceUri("sb", ConfigurationManager.AppSettings["ServiceBus.Namespace"], ""), new MessagingFactorySettings()
						{
							TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(ConfigurationManager.AppSettings["ServiceBus.KeyName"], ConfigurationManager.AppSettings["ServiceBus.Key"]),
							TransportType = TransportType.Amqp
						});

						var client = factory.CreateEventHubClient("callstatsout");
						var group = client.GetDefaultConsumerGroup();

						Console.WriteLine("Group: {0}", group.GroupName);

						var receiver = group.CreateReceiver(state.ToString(), DateTime.UtcNow);

						while (true)
						{
							if (cts.IsCancellationRequested)
							{
								receiver.Close();
								break;
							}

							var messages = receiver.Receive(10);
							foreach (var item in messages)
							{
								string body = Encoding.Default.GetString(item.GetBytes());

								body = string.Format("[{0}]", body.Replace("}{", "},{"));

								var callstats = JsonConvert.DeserializeObject<List<CallStats>>(body);

								foreach (var stat in callstats)
								{
									Console.WriteLine("[{5}] StartWindow={0} EndWindow={1} Started={2} Ended={3} Dropped={4} NotAnsweredBusy={6}",
										stat.WinStartTime, stat.WinEndTime, stat.Started, stat.Ended,
										stat.Dropped, stat.ConnectionName, stat.NotAnsweredBusy);
									hub.Clients.All.update(stat.WinEndTime.ToString("yyyy/MM/dd HH:mm:ss"), stat.Started, stat.Ended, stat.Dropped, stat.NotAnsweredBusy);
								}
							}
						}
					}, i);
			}

			Console.ReadLine();
			cts.Cancel();
		}
	}
}
