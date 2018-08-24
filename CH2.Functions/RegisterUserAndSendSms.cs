using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using SendGrid.Helpers.Mail;
using Twilio;
using Newtonsoft.Json;
using System.IO;
using CH2.Functions.Shared;

//Install nuget pkg Microsoft.Azure.WebJobs.Extensions.Twilio

namespace CH2.Functions
{
    public static class RegisterUserAndSendSms
    {
        [FunctionName("RegisterUserAndSendSms")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            [Table("UserProfile")] CloudTable userProfileTable,
            [Queue("emaillogqueue")] CloudQueue queue,
            IBinder binder,
            [SendGrid()] IAsyncCollector<Mail> message,
            [TwilioSms()] IAsyncCollector<SMSMessage> smsMessage, 
            TraceWriter log)
        {
            try
            {
                log.Info($"RegisterUserAndSendSms function processed a request.");

                var user = JsonConvert.DeserializeObject<UserProfile>(await req.Content.ReadAsStringAsync());
                if (user == null)
                    throw new System.Exception("Please provide values for First name,Last Name and Profile Picture");
                var userProfile = new UserProfile(user.FirstName, user.LastName, user.ProfilePicture, user.Email);

                var tblInsertOperation = TableOperation.Insert(userProfile);
                var insertResult = await userProfileTable.ExecuteAsync(tblInsertOperation);
                var insertedUser = (UserProfile)insertResult.Result;

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

                await queue.AddMessageAsync(new CloudQueueMessage(content.Value.ToString()));
                var queueMessage = await queue.GetMessageAsync();
                var data = queueMessage.AsString;
                using (var emailLogBlob = binder.Bind<TextWriter>(new BlobAttribute($"emailogblobcontainer/{insertedUser.RowKey}.log")))
                {
                    await emailLogBlob.WriteAsync(data);
                }

                var attachment = new Attachment();
                attachment.Content = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
                attachment.Filename = $"{insertedUser.FirstName}_{insertedUser.LastName}.log";
                mail.AddAttachment(attachment);
                mail.AddContent(content);
                mail.AddPersonalization(personalization);
                await message.AddAsync(mail);

                //SMS Code
                var sms = new SMSMessage();
                sms.To = "XXXXXXXXXXXXXXX";
                sms.From = System.Configuration.ConfigurationManager.AppSettings["AzureWebJobsFromNumber"];
                sms.Body = "How is it going!";

                await smsMessage.AddAsync(sms);

            }
            catch (System.Exception ex)
            {
                log.Info($"RegisterUserAndSendSms function : Exception:{ ex.Message}");
                throw;
            }
            finally
            {
                log.Info("RegisterUserAndSendSms function has finished processing a request.");
            }
        }
    }
}
