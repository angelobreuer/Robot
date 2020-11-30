namespace Robot.Server
{
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = Host.CreateDefaultBuilder(args);

            webHost.ConfigureWebHostDefaults(builder =>
            {
                builder.UseContentRoot(Directory.GetCurrentDirectory());
                builder.UseStartup<Startup>();
            });

            webHost.Build().Run();
        }
    }
}
