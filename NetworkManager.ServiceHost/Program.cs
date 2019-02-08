using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkManager.Model.Geography;
using NetworkManager.Model.Topology;
using NetworkManager.Web.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using Topshelf;

namespace NetworkManager.ServiceHost
{
    // Used to inject in the MVC framework
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // Register Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMvc();
        }
    }

    /// <summary>
    /// This is the implementation of the windows service
    /// </summary>
    public class NetworkManagerService
    {
        private IWebHost _host;

        public NetworkManagerService(NetworkTopology topology)
        {
            // Spin the host up
            _host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            // Configure the controllers
            TopologyController.Topology = topology;
            GeographyController.Geography = new NetworkGeography(topology);
        }

        public void Start()
        {
            // Start responding to requests
            _host.Run();
            Console.WriteLine("Service Started");
        }

        public void Stop()
        {
            // Stop responding to requests
            Console.WriteLine("Service Stopping");
            _host.StopAsync().Wait();
            Console.WriteLine("Service Stopped");
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            // Read appsettings.json
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var config = builder.Build();

            // Pull in the network topology object
            var networkModelPath = config["AppSettings:NetworkModelPath"];
            using (var fs = new FileStream(networkModelPath, FileMode.Open))
            {
                Console.WriteLine("Loading Network Topology...");
                var networkTopology = new NetworkTopology(fs);
                Console.WriteLine("Network Topology Loaded");

                var rc = HostFactory.Run(x =>
                {
                    x.Service<NetworkManagerService>(s =>
                    {
                        s.ConstructUsing(name => new NetworkManagerService(networkTopology));
                        s.WhenStarted(tc => tc.Start());
                        s.WhenStopped(tc => tc.Stop());
                    });
                    x.RunAsLocalSystem();

                    x.SetDescription("NetworkManager Self-Hosted REST API");
                    x.SetDisplayName("NetworkManager.ServiceHost");
                    x.SetServiceName("NetworkManager.ServiceHost");
                });

                var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
                Environment.ExitCode = exitCode;
            }
        }
    }
}
