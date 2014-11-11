using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace CallProducer
{


	public class CallManager
	{
		private int currentTick;
		private Timer timer;
		private List<LinkState> links;
		private int numLinks;
		private readonly EventHubClient client;
		private string connectionName;

		public CallManager(int numOfLinks, string eventHubName, string connectionName)
		{
			this.numLinks = numOfLinks;
			this.links = new List<LinkState>();
			this.timer = new Timer(5000);
			this.timer.Elapsed += timer_Elapsed;
			this.client = EventHubClient.Create(eventHubName);
			this.connectionName = connectionName;
		}

		private void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			this.currentTick++;

			Random rnd = new Random();

			//Stop a random number of calls
			int numCallsToStop = rnd.Next(0, 20);
			var callsToStop = this.links.OrderBy(m => rnd.Next()).Take(numCallsToStop);

			callsToStop.ToList().ForEach(l =>
				{
					var evt = new PhoneCallEvent()
					{
						Id = l.Id,
						EventType = 2,
						ConnectionName = connectionName
					};
					Send(evt);
					//produce an event
					//Console.WriteLine("Stopping call: " + l.Id);
				});
			Console.WriteLine("Stopping: " + callsToStop.Count());

			this.links = this.links.Except(callsToStop).ToList();

			//Drop a random number of calls
			int numCallsToDrop = rnd.Next(0, 5);
			var callsToDrop = this.links.OrderBy(m => rnd.Next()).Take(numCallsToDrop);

			callsToDrop.ToList().ForEach(l =>
			{
				var evt = new PhoneCallEvent()
				{
					Id = l.Id,
					EventType = 3,
					ConnectionName = connectionName
				};
				Send(evt);

				//produce an event
				//Console.WriteLine("Dropping call: " + l.Id);
			});
			Console.WriteLine("Dropping: " + callsToDrop.Count());

			this.links = this.links.Except(callsToDrop).ToList();

			//Start a random number of calls
			int numCallsToStart = rnd.Next(10, 50);

			if (numCallsToStart > this.links.Count && numCallsToStart - this.links.Count >= this.numLinks)
			{
				Console.WriteLine("Unable to start any new calls");
			}
			else
			{
				var callsToStart = Enumerable.Range(0, numCallsToStart).Select(i => new LinkState());
				int started = 0;
				int busy = 0;

				callsToStart.ToList().ForEach(l =>
					{

						if (links.Count < this.numLinks)
						{
							var evt = new PhoneCallEvent()
							{
								Id = l.Id,
								EventType = 1,
								ConnectionName = connectionName
							};
							Send(evt);
							//Console.WriteLine("Starting call: " + l.Id);
							started++;
							this.links.Add(l);
						}
						else
						{
							var evt = new PhoneCallEvent()
							{
								Id = l.Id,
								EventType = 4,
								ConnectionName = connectionName
							};
							Send(evt);
							busy++;
							//Console.WriteLine("Not answered, all links busy");
						}
					});
				Console.WriteLine("Started: " + started);
				Console.WriteLine("Busy: " + busy);
			}

			Console.WriteLine("Calls in progress: " + links.Count);
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}

		private void Send(PhoneCallEvent evt)
		{
			var json = JsonConvert.SerializeObject(evt);
			EventData data = new EventData(Encoding.UTF8.GetBytes(json))
			{
				PartitionKey = "1"
			};

			data.Properties.Add("Type", "Telemetry_" + DateTime.Now.ToLongTimeString());

			try
			{
				client.Send(data);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error on send: " + ex.Message);
			}
		}

		internal void StartMakingCalls()
		{
			timer_Elapsed(null, null);
			timer_Elapsed(null, null);
			timer.Start();
		}
	}

	public class LinkState
	{
		public LinkState()
		{
			Id = Guid.NewGuid();
			Start = DateTime.Now;
		}
		public Guid Id { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}

	public class PhoneCallEvent
	{
		public string ConnectionName { get; set; }
		public Guid Id { get; set; }
		public int EventType { get; set; }
	}
}
