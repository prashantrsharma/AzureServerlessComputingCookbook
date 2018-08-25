using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpartanEventHubConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
           EventHubHelper.GenerateHubMessages().Wait();
           
        }
    }
}
