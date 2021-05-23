using Hangfire;
using Hangfire.Console;
using System;

namespace TextumReader.HangfireJobServer
{
    class Program
    {
        static void Main(string[] args)
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("Server=.\\sqlexpress;Database=HangfireGoogleTranslateScrapper;Integrated Security=SSPI")
                .UseConsole();

            using (var server = new BackgroundJobServer())
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
