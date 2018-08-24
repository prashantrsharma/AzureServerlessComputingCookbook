using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;

namespace CH1.Functions
{
    public static class UploadProfilePictureToBlob
    {
        [FunctionName("UploadProfilePictureToBlob")]
        public static async Task Run([QueueTrigger("userprofileimagesqueue")]string queueItem, [Blob("userprofileimagecontainer/{rand-guid}", FileAccess.Write)] Stream blob, TraceWriter log)
        {
            try
            {
                log.Info($"UploadProfilePictureToBlob Processed blob\n Name:{queueItem} \n Size: {queueItem.Length} Bytes");

                using (var httpClient = new HttpClient())
                {
                    var data = await httpClient.GetByteArrayAsync(queueItem);
                    await blob.WriteAsync(data, 0, data.Length);
                }
            }
            catch (System.Exception ex)
            {
                log.Info($"UploadProfilePictureToBlob function : Exception:{ ex.Message}");
                throw;
            }
            finally
            {
                log.Info("UploadProfilePictureToBlob function has finished processing a request.");
            }
        }
    }
}
