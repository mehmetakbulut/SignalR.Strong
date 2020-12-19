using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong.Tests.Common;
using Xunit;

namespace SignalR.Strong.Tests.xUnit
{
    public class ClientToServerStream : IClassFixture<ServerFixture>
    {
        private ServerFixture fixture;

        public ClientToServerStream(ServerFixture fixture)
        {
            this.fixture = fixture;
        }
        
        [Fact]
        public async Task TxWithoutToken()
        {
            var client = await fixture.GetClient();
            var hub = client.GetHub<IMockHub>();

            var data = new List<int>() {1, 2, 3};
            var channel = Channel.CreateUnbounded<int>();
            await hub.StreamFromClient(data, channel.Reader);
        }
        
        [Fact]
        public async Task Loop()
        {
            var client = await fixture.GetClient();
            var hub = client.GetHub<IMockHub>();

            var channel = Channel.CreateUnbounded<int>();

            var cts = new CancellationTokenSource();

            await hub.LoopReset();

            await hub.LoopTx(channel.Reader);
            
            var echo = await hub.LoopRx();

            int n_loop = 3;

            for (int i = 0; i < n_loop; i++)
            {
                await channel.Writer.WriteAsync(i, cts.Token);
            }
            
            for (int i = 0; i < n_loop; i++)
            {
                await echo.ReadAsync(cts.Token);
            }

            channel.Writer.Complete();
        }
    }
}