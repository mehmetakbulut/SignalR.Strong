using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong.Tests.Common;
using Xunit;

namespace SignalR.Strong.Tests.xUnit
{
    public class ServerToClientStream : IClassFixture<ServerFixture>
    {
        private ServerFixture fixture;
        
        public ServerToClientStream(ServerFixture fixture)
        {
            this.fixture = fixture;
        }
        
        [Fact]
        public async Task RxWithoutToken()
        {
            var client = await fixture.GetClient();
            var hub = client.GetHub<IMockHub>();

            var data = new List<int>() {1, 2, 3};
            var reader = await hub.StreamToClient(data);

            foreach (var item in data)
            {
                await reader.WaitToReadAsync();
                reader.TryRead(out var recv).Should().BeTrue();
                recv.Should().Be(item);
            }
        }
        
        [Fact]
        public async Task RxWithToken()
        {
            var client = await fixture.GetClient();
            var hub = client.GetHub<IMockHub>();

            var data = new List<int>() {1, 2, 3};
            var cts = new CancellationTokenSource();
            var reader = await hub.StreamToClientWithToken(data, cts.Token);

            await reader.WaitToReadAsync();
            reader.TryRead(out var recv).Should().BeTrue();
            recv.Should().Be(data[0]);
            cts.Cancel();
            var delay = Task.Delay(1000);   // Allow 1s before timing out
            // TODO: Completion might get messy if MockHub doesn't cancel in time but sends a 2nd item before timeout
            await Task.WhenAny(reader.Completion, delay);
            if (reader.Completion.IsCompleted == false)
            {
                reader.Completion.Dispose();
                delay.Dispose();
                throw new AssertionFailedException("Token didn't cause channel to get closed within timeout");
            }
        }
    }
}