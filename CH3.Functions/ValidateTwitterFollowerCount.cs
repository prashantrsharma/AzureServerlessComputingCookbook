using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace CH3.Functions
{
    public static class ValidateTwitterFollowerCount
    {
        [FunctionName("ValidateTwitterFollowerCount")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            [SendGrid()] IAsyncCollector<Mail> message,
            TraceWriter log)
        {
            try
            {
                log.Info("ValidateTwitterFollowerCount function processed a request.");

                var tweetData = new
                {
                    TweetText = "",
                    FollowersCount = "",
                    Name = ""

                };

                var tweet = JsonConvert.DeserializeAnonymousType(await req.Content.ReadAsStringAsync(), tweetData);

                if(Convert.ToInt32(tweet.FollowersCount) > 100)
                {
                    Mail mail = new Mail()
                    {
                        Subject = $"{tweet.Name} with  {tweet.FollowersCount} followers has posted a tweet!"

                    };

                    Content content = new Content
                    {
                        Type = "text/plain",
                        Value = $" Tweet Content : {tweet.TweetText}"
                    };

                    mail.AddContent(content);

                    var personalization = new Personalization();
                    personalization.AddTo(new Email(System.Environment.GetEnvironmentVariable("EmailTo")));
                    mail.AddPersonalization(personalization);

                    await message.AddAsync(mail);

                }

                return req.CreateResponse(HttpStatusCode.OK);

            }
            catch (System.Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, $"We apologize but something went wrong on our end.Exception:{ex.Message}");
            }
            finally
            {
                log.Info("ValidateTwitterFollowerCount function has finished processing a request.");
            }
           
        }
    }
}
