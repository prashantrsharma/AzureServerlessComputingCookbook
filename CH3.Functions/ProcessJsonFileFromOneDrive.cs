using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace CH3.Functions
{
    public static class ProcessJsonFileFromOneDrive
    {
        [FunctionName("ProcessJsonFileFromOneDrive")]
        public static void  Run(
            string inputFileName,
            string name,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            log.Info(inputFileName);
          
        }
    }
}
