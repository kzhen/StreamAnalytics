using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ServiceBus.Samples
{
	using System.Configuration;
	using Microsoft.ServiceBus.Messaging;
	using Microsoft.ServiceBus.Samples.SessionMessages;

	class Program
	{
		#region Fields

		static string eventHubName;
		static int numberOfMessages;
		static int numberOfPartitions;
		#endregion

		static void Main(string[] args)
		{
			//Initialize
			ParseArgs(args);
			string connectionString = GetServiceBusConnectionString();
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

			Manage.CreateEventHub(eventHubName, numberOfPartitions, namespaceManager);

			//Sender s = new Sender(eventHubName, numberOfMessages);
			//s.SendEvents();

			PhoneCallProducer producer = new PhoneCallProducer(eventHubName);

			Console.WriteLine("Ready...");
			Console.WriteLine("To send start: START <phonenumber> <id>");
			Console.WriteLine("To send end: END <phonenumber> <id>");
			Console.WriteLine("To exit: EXIT");
			Console.WriteLine();

			while (true)
			{
				var cmd = Console.ReadLine();

				if (cmd.ToUpper().Equals("EXIT"))
				{
					break;
				}

				if (cmd.StartsWith("START"))
				{
					var phoneIdx = cmd.IndexOf(' ');
					var idIdx = cmd.IndexOf(' ', phoneIdx + 1);
					string phoneNumber = cmd.Substring(phoneIdx, idIdx - phoneIdx);
					string id = cmd.Substring(idIdx);
					
					producer.StartCall(phoneNumber, id);
				}
				else if (cmd.StartsWith("END"))
				{
					var phoneIdx = cmd.IndexOf(' ');
					var idIdx = cmd.IndexOf(' ', phoneIdx + 1);
					string phoneNumber = cmd.Substring(phoneIdx, idIdx - phoneIdx);
					string id = cmd.Substring(idIdx);

					producer.EndCall(phoneNumber, id);
				}
			}

			Console.WriteLine("Press enter key to stop worker.");
			Console.ReadLine();
		}

		static void ParseArgs(string[] args)
		{
			if (args.Length < 2)
			{
				throw new ArgumentException("Incorrect number of arguments. Expected 3 args <eventhubname> <NumberOfMessagesToSend> <NumberOfPartitions>", args.ToString());
			}
			else
			{
				eventHubName = args[0];
				Console.WriteLine("ehnanme: " + eventHubName);

				numberOfMessages = Int32.Parse(args[1]);
				Console.WriteLine("NumberOfmessage: " + numberOfMessages);

				numberOfPartitions = Int32.Parse(args[2]);
				Console.WriteLine("numberOfPartitions: " + numberOfPartitions);
			}
		}

		private static string GetServiceBusConnectionString()
		{
			string connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
			if (string.IsNullOrEmpty(connectionString))
			{
				Console.WriteLine("Did not find Service Bus connections string in appsettings (app.config)");
				return string.Empty;
			}
			ServiceBusConnectionStringBuilder builder = new ServiceBusConnectionStringBuilder(connectionString);
			builder.TransportType = TransportType.Amqp;
			return builder.ToString();
		}
	}
}
