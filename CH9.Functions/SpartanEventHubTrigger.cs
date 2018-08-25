using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;

namespace CH9.Functions
{
    public static class SpartanEventHubTrigger
    {
        [FunctionName("SpartanEventHubTrigger")]
        public static void Run([EventHubTrigger("spartaneventhub", Connection = "EventHubConnection")]string myEventHubMessage, TraceWriter log)
        {
            try
            {
                log.Info($"SpartanEventHubTrigger function processed a message: {myEventHubMessage}");
            }
            catch (System.Exception ex)
            {
                log.Info($"SpartanEventHubTrigger function generated an exception:{ex.Message}");
                throw;
            }
            finally
            {
                log.Info("UploadPicToBlob function has finished processing a request.");
            }
        }
    }
}
