// Setup
// 1) Go to https://www.microsoft.com/cognitive-services/en-us/computer-vision-api 
//    Sign up for computer vision api
// 2) Go to Platform features -> Application settings
//    create a new app setting Vision_API_Subscription_Key and use Computer vision key as value

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace CH3.Functions
{
    public static class LocateMaleFemaleFaces
    {
        [FunctionName("LocateMaleFemaleFaces")]
        public static async Task Run(
            [BlobTrigger("images/{name}.jpg")]Stream image, 
            string name, 
            [Table("MaleFaceRectangle")]IAsyncCollector<FaceRectangle> outMaleTable,
            [Table("FemaleFaceRectangle")]IAsyncCollector<FaceRectangle> outFemaleTable,
            TraceWriter log)
        {
            string result = await CallVisionAPI(image);
            log.Info(result);

            if (String.IsNullOrEmpty(result))
            {
                return;
            }

            ImageData imageData = JsonConvert.DeserializeObject<ImageData>(result);
            foreach (Face face in imageData.Faces)
            {
                var faceRectangle = face.FaceRectangle;
                faceRectangle.RowKey = Guid.NewGuid().ToString();
                faceRectangle.PartitionKey = "Functions";
                faceRectangle.ImageFile = name + ".jpg";
                if(face.Gender == "Male")
                {
                    await outMaleTable.AddAsync(faceRectangle);
                }
                else if(face.Gender == "Female")
                {
                    await outFemaleTable.AddAsync(faceRectangle);
                }
               
            }
        }

        static async Task<string> CallVisionAPI(Stream image)
        {
            using (var client = new HttpClient())
            {
                var content = new StreamContent(image);
                var url = "https://eastus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Faces&language=en";
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("Vision_API_Subscription_Key"));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var httpResponse = await client.PostAsync(url, content);

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    return await httpResponse.Content.ReadAsStringAsync();
                }
            }
            return null;
        }

        public class ImageData
        {
            public List<Face> Faces { get; set; }
        }

        public class Face
        {
            public int Age { get; set; }

            public string Gender { get; set; }

            public FaceRectangle FaceRectangle { get; set; }
        }

        public class FaceRectangle : TableEntity
        {
            public string ImageFile { get; set; }

            public int Left { get; set; }

            public int Top { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }
        }
    }
}
