using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Search.JavaOPlusD.Api
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var componentUrl = Environment.GetEnvironmentVariable("java_oplusd_url");
            if (string.IsNullOrEmpty(componentUrl))
            {
                Console.WriteLine("Connection string to OPlusD instance must be passed as environment variable 'postgres_connection'");
                return 1;
            }

            using (var searchProvider = new JavaOPlusDSearchProvider(componentUrl))
            { 
                ApiCore.Api.BuildHost(args, searchProvider).Run();
            }
            return 0;
        }
    }
}
