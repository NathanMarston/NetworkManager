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
using System.Threading.Tasks;
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

        public NetworkManagerService()
        {
            // Spin the host up
            _host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();
        }

        public void LoadDependencies(string networkModelPath)
        {
            Console.WriteLine("Loading network model...");
            using (var fs = new FileStream(networkModelPath, FileMode.Open))
            {
                var model = new NetworkTopology(fs);
                TopologyController.Topology = model;
                GeographyController.Geography = new NetworkGeography(model);
            }
            Console.WriteLine("Network model loaded!");
            _host.Start();
        }

        public void StartService(string networkModelPath)
        {
            // Start responding to requests
            Console.WriteLine("Service Started");
            Task.Run(() => LoadDependencies(networkModelPath));
        }

        public void StopService()
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

            // Pull in the network model
            var networkModelPath = config["AppSettings:NetworkModelPath"];
            var service = new NetworkManagerService();

            var rc = HostFactory.Run(x =>
            {
                x.Service<NetworkManagerService>(s =>
                {
                    s.ConstructUsing(name => service);
                    s.WhenStarted(tc => tc.StartService(networkModelPath));
                    s.WhenStopped(tc => tc.StopService());
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
