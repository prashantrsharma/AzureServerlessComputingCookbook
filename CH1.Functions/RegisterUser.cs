using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CH1.Functions.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;


namespace CH1.Functions
{
    public static class RegisterUser
    {
        [FunctionName("RegisterUser")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                log.Info("RegisterUser function processed a request.");

                var user = JsonConvert.DeserializeObject<User>(await req.Content.ReadAsStringAsync());

                if (user == null)
                    return req.CreateResponse(HttpStatusCode.BadRequest,"Please enter value for First name and Last Name");

                return req.CreateResponse(HttpStatusCode.OK, $"Hello {user.FirstName} {user.LastName} !");
            }
            catch (System.Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, $"We apologize but something went wrong on our end.Exception:{ex.Message}");
            }
            finally
            {
                log.Info("RegisterUser function has finished processing a request.");
            }
        }
    }
}
