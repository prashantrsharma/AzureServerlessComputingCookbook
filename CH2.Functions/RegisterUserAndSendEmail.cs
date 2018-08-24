// The 'From' and 'To' fields are automatically populated with the values specified by the binding settings.
//
// You can also optionally configure the default From/To addresses globally via host.config, e.g.:
//
// {
//   "sendGrid": {
//      "to": "user@host.com",
//      "from": "Azure Functions <samples@functions.com>"
//   }
// }
using CH2.Functions.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using System.Net.Http;
using System.Threading.Tasks;

namespace CH2.Functions
{
    public static class RegisterUserAndSendEmail
    {
        [FunctionName("RegisterUserAndSendEmail")]
        public static async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, 
            [Table("UserProfile")] CloudTable userProfileTable,
            [SendGrid()] ICollector<Mail> message, 
            TraceWriter log)
        {

            try
            {
                log.Info($"RegisterUserAndSendEmail function processed a request.");

                var user = JsonConvert.DeserializeObject<UserProfile>(await req.Content.ReadAsStringAsync());

                if (user == null)
                    throw new System.Exception("Please provide values for First name,Last Name and Profile Picture");

                var userProfile = new UserProfile(user.FirstName, user.LastName, user.ProfilePicture,string.Empty);

                TableOperation tableOperationInsert = TableOperation.Insert(userProfile);

                userProfileTable.Execute(tableOperationInsert);

                Mail mail = new Mail()
                {
                    Subject = $"Thanks {userProfile.FirstName} {userProfile.LastName} for your Sign Up!"
                    
                };

                //var personalization = new Personalization();
                //personalization.AddTo(new Email("the-email-address-of-recipient"));

                Content content = new Content
                {
                    Type = "text/plain",
                    Value = $"{userProfile.FirstName}, here's the link for your profile picture {userProfile.ProfilePicture}!"
                };

                mail.AddContent(content);

                message.Add(mail);
            }
            catch (System.Exception ex)
            {
                log.Info($"RegisterUserAndSendEmail function : Exception:{ ex.Message}");
                throw;
            }
            finally
            {
                log.Info("RegisterUserAndSendEmail function has finished processing a request.");
            }
           
        }
    }
}
