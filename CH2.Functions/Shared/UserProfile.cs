using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH2.Functions.Shared
{
    public class UserProfile : TableEntity
    {
        public UserProfile()
        {
            PartitionKey = ConfigurationManager.AppSettings["PartitionKey"].ToString();
            RowKey = Guid.NewGuid().ToString();
        }

        public UserProfile(string firstName, string lastName) : this()
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public UserProfile(string firstName, string lastName, string profilePicture, string email) : this()
        {
            FirstName = firstName;
            LastName = lastName;
            ProfilePicture = profilePicture;
            Email = email;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicture { get; set; }
        public string Email { get; set; }
    }
}


