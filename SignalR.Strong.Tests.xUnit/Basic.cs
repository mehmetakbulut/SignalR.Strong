using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using SignalR.Strong.Samples.Common.Hubs;
using SignalR.Strong.Tests.Common;
using Xunit;

namespace SignalR.Strong.Tests.xUnit
{
    public class Basic : IClassFixture<ServerFixture>
    {
        private ServerFixture fixture;

        public Basic(ServerFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetDynamicHub()
        {
            var conn = await fixture.GetHubConnection();
            var getHubStrong = conn.AsDynamicHub<IMockHub>();
            var getHubWeak = conn.AsDynamicHub(typeof(IMockHub));
        }

        [Fact]
        public async Task GetExpressiveHub()
        {
            var conn = await fixture.GetHubConnection();
            var getExpressiveHubStrong = conn.AsExpressiveHub<IMockHub>();
        }
    }
}