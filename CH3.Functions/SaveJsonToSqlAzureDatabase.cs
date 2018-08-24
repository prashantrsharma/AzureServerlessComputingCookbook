using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace CH3.Functions
{
    public static class SaveJsonToSqlAzureDatabase
    {
        [FunctionName("SaveJsonToSqlAzureDatabase")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            TraceWriter log)
        {
            try
            {
                log.Info("SaveJsonToSqlAzureDatabase function processed a request.");

                var EmployeeInfo = new
                {
                    FirstName = "",
                    LastName = "",
                    Email = "",
                    Devicelist = new[]
                    {
                        new
                        {
                            Type = "",
                            Company = ""
                        }
                    }
                };

                var employee = JsonConvert.DeserializeAnonymousType(await req.Content.ReadAsStringAsync(), EmployeeInfo);

                var query = "Insert into [dbo].[EmployeeInfo] values (@firstname,@lastname, @email, @devicelist);";

                using (var conn = new SqlConnection(System.Environment.GetEnvironmentVariable("VidlyEssentials_ConnectionString")))
                {
                    var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@firstname", employee.FirstName);
                    cmd.Parameters.AddWithValue("@lastname", employee.LastName);
                    cmd.Parameters.AddWithValue("@email", employee.Email);
                    var keyValuePairs = employee.Devicelist.Select(x => string.Format("{0}{1}{2}", x.Type, ":", x.Company));
                    cmd.Parameters.AddWithValue("@devicelist", string.Join(",", keyValuePairs));
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }

                return req.CreateResponse(HttpStatusCode.OK);


            }
            catch (System.Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, $"We apologize but something went wrong on our end.Exception:{ex.Message}");

            }
            finally
            {
                log.Info("SaveJsonToSqlAzureDatabase function has finished processing a request.");
            }
        }
    }
}
