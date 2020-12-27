using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using SignalR.Strong.Tests.Common;
using Xunit;

namespace SignalR.Strong.Tests.xUnit.Streams
{
    public class Expr : IClassFixture<ServerFixture>
    {
        private ServerFixture fixture;

        public Expr(ServerFixture fixture)
        {
            this.fixture = fixture;
        }
        
        [Fact]
        public async Task ClientToServerChannel()
        {
            var conn = await fixture.GetHubConnection();
            var ehub = conn.AsExpressiveHub<IMockHub>();

            var data = new List<int>() {1, 2, 3};
            var reader = await ehub.StreamAsChannelAsync(hub => hub.StreamToClientViaChannel(data));

            foreach (var item in data)
            {
                await reader.WaitToReadAsync();
                reader.TryRead(out var recv).Should().BeTrue();
                recv.Should().Be(item);
            }
        }
        
        [Fact]
        public async Task ClientToServerChannelWithToken()
        {
            var conn = await fixture.GetHubConnection();
            var ehub = conn.AsExpressiveHub<IMockHub>();

            var data = new List<int>() {1, 2, 3};
            var cts = new CancellationTokenSource();
            var reader = await ehub.StreamAsChannelAsync(hub => hub.StreamToClientViaChannelWithToken(data, cts.Token));

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
        
        [Fact]
        public async Task ClientToServerEnumerableWithToken()
        {
            var conn = await fixture.GetHubConnection();
            var ehub = conn.AsExpressiveHub<IMockHub>();

            var data = new List<int>() {1, 2, 3};
            var cts = new CancellationTokenSource();
            var reader = ehub.StreamAsync(hub => hub.StreamToClientViaEnumerableWithToken(data, cts.Token));

            int i = 0;
            await foreach (var item in reader)
            {
                item.Should().Be(data[i]);
                i++;
            }
        }

        [Fact]
        public async Task ServerToClientChannelReader()
        {
            var conn = await fixture.GetHubConnection();
            var ehub = conn.AsExpressiveHub<IMockHub>();

            var data = new List<int>() {1, 2, 3};
            var channel = Channel.CreateUnbounded<int>();
            await ehub.SendAsync(hub => hub.StreamFromClientViaChannel(data, channel.Reader));
        }

        [Fact]
        public async Task ServerToClientEnumerable()
        {
            var conn = await fixture.GetHubConnection();
            var ehub = conn.AsExpressiveHub<IMockHub>();

            var data = new List<int>() {1, 2, 3};

            async IAsyncEnumerable<int> clientStreamData()
            {
                yield return 0;
                await Task.Yield();
            }

            var reader = clientStreamData();

            await ehub.SendAsync(hub => hub.StreamFromClientViaEnumerable(data, reader));
        }
    }
}