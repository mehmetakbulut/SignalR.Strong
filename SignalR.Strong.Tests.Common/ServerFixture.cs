using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace SignalR.Strong.Tests.Common
{
    // Based on https://www.dotnetcurry.com/aspnet-core/1519/integration-testing-signalr-kestrel
    public class ServerFixture
    {
        public const string BaseUrl = "http://localhost:54321";
        
        static ServerFixture()
        {
            var webhost = WebHost
                .CreateDefaultBuilder(null)
                .UseStartup<Startup>()
                .UseUrls(BaseUrl)
                .Build();
  
            webhost.Start();
        }
  
        public string GetCompleteServerUrl(string route)
        {
            route = route?.TrimStart('/', '\\');
            return $"{BaseUrl}/{route}";
        }

        public class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddSignalR();
            }

            public void Configure(IApplicationBuilder app)
            {
                app.UseDeveloperExceptionPage();

                app.UseSignalR(builder =>
                {
                    builder.MapHub<MockHub>("/mockHub");
                });
            }
        }
    }

    public static class SignalRpcClientExtensions
    {
        public static async Task<StrongClient> GetClient(this ServerFixture fixture)
        {
            var conn = new HubConnectionBuilder()
                .WithUrl(fixture.GetCompleteServerUrl("/mockhub"))
                .Build();
            var client = new StrongClient();
            (await client.RegisterHub<IMockHub>(conn).ConnectToHubsAsync()).BuildSpokes();
            return client;
        }
    }
}
