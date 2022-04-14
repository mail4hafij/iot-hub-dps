using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace BuiltInEndPoint
{
    public static class BuiltInEndPoint
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("BuiltInEndPoint")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "IotHubConnection")]EventData message, ILogger log)
        {
            string messageBody = Encoding.UTF8.GetString(message.Body.Array, message.Body.Offset, message.Body.Count);
            log.LogInformation($"BuiltInEndPoint: {messageBody} \n");
        }
    }
}