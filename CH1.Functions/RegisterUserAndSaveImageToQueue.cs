using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CH1.Functions.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace CH1.Functions
{
    public static class RegisterUserAndSaveImageToQueue
    {
        [FunctionName("RegisterUserAndSaveImageToQueue")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            TraceWriter log, [Table("UserProfile")] CloudTable userProfileTable,[Queue("userprofileimagesqueue")] ICollector<string> outputQueueItem)
        {
            try
            {
                log.Info("RegisterUserAndSaveImageToQueue function processed a request.");

                var userProfile = JsonConvert.DeserializeObject<UserProfile>(await req.Content.ReadAsStringAsync());

                if (userProfile == null)
                    return req.CreateResponse(HttpStatusCode.BadRequest, "Please provide values for First name,Last Name and Profile Picture");

                TableOperation tableOperationInsert = TableOperation.Insert(userProfile);

                userProfileTable.Execute(tableOperationInsert);

                outputQueueItem.Add(userProfile.ProfilePicture);

                return req.CreateResponse(HttpStatusCode.OK);

            }
            catch (System.Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, $"We apologize but something went wrong on our end.Exception:{ex.Message}");
            }
            finally
            {
                log.Info("RegisterUserAndSaveImageToQueue function has finished processing a request.");
            }
        }
    }
}
