using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ServiceBus.Samples
{
	internal class PhoneCallProducer
	{
		private readonly string eventHubName;
		private readonly EventHubClient client;
		public PhoneCallProducer(string eventHubName)
		{
			this.eventHubName = eventHubName;
			this.client = EventHubClient.Create(this.eventHubName);
		}
		public void StartCall(string phoneNumber, string id)
		{
			var evt = new PhoneCallEvent()
			{
				Id = id,
				PhoneNumber = phoneNumber,
				EventType = 1
			};

			SendMessage(evt);
		}

		private void SendMessage(PhoneCallEvent evt)
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
				Console.WriteLine("Error on send START: " + ex.Message);
			}
		}

		public void EndCall(string phoneNumber, string id)
		{
			var evt = new PhoneCallEvent()
			{
				Id = id,
				PhoneNumber = phoneNumber,
				EventType = 2
			};

			try
			{
				SendMessage(evt);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error on send END: " + ex.Message);
			}
		}
	}

	internal class PhoneCallEvent
	{
		public string Id { get; set; }
		public string PhoneNumber { get; set; }
		public int EventType { get; set; }
	}
}
