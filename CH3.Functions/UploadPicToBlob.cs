using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using Newtonsoft.Json;

namespace CH3.Functions
{
    public static class UploadPicToBlob
    {
        [FunctionName("UploadPicToBlob")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            [Blob("images/{rand-guid}.jpg", System.IO.FileAccess.Write)] Stream blob,
            TraceWriter log)
        {
            try
            {
                log.Info("UploadPicToBlob function processed a request.");

                var picture = new { profilePicture = "" };

                var image = JsonConvert.DeserializeAnonymousType(await req.Content.ReadAsStringAsync(),picture);

                using (var httpClient = new HttpClient())
                {
                    var data = await httpClient.GetByteArrayAsync(image.profilePicture);
                    await blob.WriteAsync(data,0,data.Length);
                }

                return req.CreateResponse(HttpStatusCode.OK);


            }
            catch (System.Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, $"We apologize but something went wrong on our end.Exception:{ex.Message}");
              
            }
            finally
            {
                log.Info("UploadPicToBlob function has finished processing a request.");
            }
        }
    }
}
