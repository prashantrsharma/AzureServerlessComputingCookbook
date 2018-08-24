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
using System.Configuration;

namespace CH1.Functions
{
    public static class RegisterTableToTableStorage
    {
        [FunctionName("RegisterTableToTableStorage")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            TraceWriter log,[Table("UserProfile")] CloudTable userProfileTable)
        {
            log.Info("RegisterTableToTableStorage function processed a request.");
            try
            {
                var user = JsonConvert.DeserializeObject<User>(await req.Content.ReadAsStringAsync());

                if (user == null)
                    return req.CreateResponse(HttpStatusCode.BadRequest, "Please enter value for First name and Last Name");

                var userProfile = new UserProfile(user.FirstName, user.LastName);

                TableOperation tableOperationInsert = TableOperation.Insert(userProfile);

                userProfileTable.Execute(tableOperationInsert);

                return req.CreateResponse(HttpStatusCode.OK, $"Hello {userProfile.FirstName} {userProfile.LastName} ,Thank you for registering....!");
            }
            catch (System.Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, $"We apologize but something went wrong on our end.Exception:{ex.Message}");
            }
            finally
            {
                log.Info("RegisterTableToTableStorage function has finished processing a request.");
            }
        }
    }
}
