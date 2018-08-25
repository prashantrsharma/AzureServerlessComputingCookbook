using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Azure.EventHubs;

namespace SpartanEventHubConsoleApp
{
    public class EventHubHelper
    {
        static EventHubClient _eventHubClient = null;

        public static async Task GenerateHubMessages()
        {
            EventHubsConnectionStringBuilder connBuilder = new EventHubsConnectionStringBuilder(ConfigurationManager.AppSettings["SpartanEventHubConnection"].ToString());
            _eventHubClient = EventHubClient.CreateFromConnectionString(connBuilder.ToString());
            for (int i = 0; i < 100; i++)
            {
                var message = Convert.ToString($"You have processed a message for:{i}");
                await _eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                Console.WriteLine(message);
                
            }
            await _eventHubClient.CloseAsync();
            Console.ReadKey();
        }
    }
}
