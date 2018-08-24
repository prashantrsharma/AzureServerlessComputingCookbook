using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using SendGrid.Helpers.Mail;
using Microsoft.WindowsAzure.Storage.Table;
using System.IO;
using Newtonsoft.Json;
using CH2.Functions.Shared;
using Microsoft.WindowsAzure.Storage.Queue;

namespace CH2.Functions
{
    public static class LogEmailInBlob
    {
        [FunctionName("LogEmailInBlob")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequestMessage req,
            [Table("UserProfile")] CloudTable userProfileTable,
            [Queue("emaillogqueue")]  CloudQueue  queue,
            [Blob("emailogblobcontainer/{rand-guid}.log")] TextWriter  logEmailBlob,
            [SendGrid()] ICollector<Mail> message,
            TraceWriter log)
        {
            try
            {
                log.Info("LogEmailInBlob function processed a request.");
                
                var user = JsonConvert.DeserializeObject<UserProfile>(await req.Content.ReadAsStringAsync());

                if (user == null)
                    throw new System.Exception("Please provide values for First name,Last Name and Profile Picture");

                var userProfile = new UserProfile(user.FirstName, user.LastName, user.ProfilePicture, user.Email);

                var tblInsertOperation = TableOperation.Insert(userProfile);

                userProfileTable.Execute(tblInsertOperation);

                var mail = new Mail
                {
                    Subject = $"Thanks {userProfile.FirstName} {userProfile.LastName} for your Sign Up!"
                };

                var personalization = new Personalization();
                personalization.AddTo(new Email(userProfile.Email));

                Content content = new Content
                {
                    Type = "text/plain",
                    Value = $"{userProfile.FirstName}, here's the link for your profile picture {userProfile.ProfilePicture}!"
                };

                mail.AddContent(content);
                mail.AddPersonalization(personalization);
                message.Add(mail);

                await queue.AddMessageAsync(new CloudQueueMessage(content.Value.ToString()));

                var queueMessage = await queue.GetMessageAsync();

                var data = queueMessage.AsString;

                await queue.DeleteMessageAsync(queueMessage);

                await logEmailBlob.WriteAsync(data);

            }
            catch (System.Exception ex)
            {
                log.Info($"LogEmailInBlob function : Exception:{ ex.Message}");
                throw;
            }
            finally
            {
                log.Info("LogEmailInBlob function has finished processing a request.");
            }
            
        }
    }
}
