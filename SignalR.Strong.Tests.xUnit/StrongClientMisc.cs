using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using SignalR.Strong.Tests.Common;
using Xunit;

namespace SignalR.Strong.Tests.xUnit
{
    public class Misc : IClassFixture<ServerFixture>
    {
        private ServerFixture fixture;

        public Misc(ServerFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetHub()
        {
            var client = await fixture.GetClient();
            var getHubStrong = client.GetHub<IMockHub>();
            var getHubWeak = client.GetHub(typeof(IMockHub));
            getHubStrong.Should().BeSameAs(getHubWeak);
        }
        
        [Fact]
        public async Task GetHub_NoDynamic()
        {
            var conn = new HubConnectionBuilder().WithUrl(fixture.GetCompleteServerUrl("/mockHub")).Build();
            
            // Case 1: Generator won't work and is properly detected
            var client1 = new StrongClient();
            var flag = client1.GetType().GetField("isDynamicCodeGenerationSupported", System.Reflection.BindingFlags.NonPublic
                                                  | System.Reflection.BindingFlags.Instance);
            flag.SetValue(client1, false);

            client1.RegisterHub<IMockHub>(conn);
            Assert.ThrowsAny<Exception>(() => client1.GetHub<IMockHub>());
            Assert.ThrowsAny<Exception>(() => client1.GetHub(typeof(IMockHub)));
            
            // Case 2: Generator won't work but is not properly detected
            var client2 = new StrongClient();
            var generator = client2.GetType().GetField("_proxyGenerator", System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);
            generator.SetValue(client2, null);

            Assert.ThrowsAny<Exception>(() => client2.RegisterHub<IMockHub>(conn));
        }
        
        [Fact]
        public async Task GetConnection()
        {
            var client = await fixture.GetClient();
            var getConnectionStrong = client.GetConnection<IMockHub>();
            var getConnectionWeak = client.GetConnection(typeof(IMockHub));
            getConnectionStrong.Should().BeSameAs(getConnectionWeak);
        }
        
        [Fact]
        public async Task GetExpressiveHub()
        {
            var client = await fixture.GetClient();
            var getExpressiveHubStrong = client.GetExpressiveHub<IMockHub>();
        }
    }
}