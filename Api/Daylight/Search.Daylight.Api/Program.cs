using System;
using Microsoft.AspNetCore.Hosting;
using Search.Daylight.Oracle;

namespace Search.Daylight.Api
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var oracleConnectionString = Environment.GetEnvironmentVariable("oracle_connection");
            if (string.IsNullOrEmpty(oracleConnectionString))
            {
                Console.WriteLine("Connection string to PostgreSQL instance must be passed as environment variable 'oracle_connection'");
                return 1;
            }

            var searchProvider = new OracleDaylightSearchProvider();

            ApiCore.Api.BuildHost(args, searchProvider).Run();
            return 0;
        }
    }
}
