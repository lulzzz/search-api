using System;
using Microsoft.AspNetCore.Hosting;
using System.Globalization;

namespace Search.JavaOPlusD.Api
{
    public class Program
    {
        public static int Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            var componentUrl = Environment.GetEnvironmentVariable("java_oplusd_url");
            if (string.IsNullOrEmpty(componentUrl))
            {
                Console.WriteLine("Connection string to OPlusD instance must be passed as environment variable 'postgres_connection'");
                return 1;
            }

            using (var searchProvider = new JavaOPlusDSearchProvider(componentUrl))
            { 
                ApiCore.Api.BuildHost(searchProvider).Run();
            }
            return 0;
        }
    }
}
