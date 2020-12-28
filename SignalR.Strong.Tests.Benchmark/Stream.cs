using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong.Expressive;
using SignalR.Strong.Tests.Common;

namespace SignalR.Strong.Tests.Benchmark
{
    [MemoryDiagnoser]
    public class Stream
    {
        public ServerFixture fixture;
        public IMockHub hub;
        public ExpressiveHub<IMockHub> ehub;
        public HubConnection conn;
        private Channel<int> channel;
        private CancellationTokenSource cts;
        private int n = 1;

        [GlobalSetup]
        public async Task SetupAsync()
        {
            fixture = new ServerFixture();
            conn = await fixture.GetHubConnection();
            hub = conn.AsDynamicHub<IMockHub>();
            ehub = conn.AsExpressiveHub<IMockHub>();
        }
        
        [Benchmark]
        public async Task GetChannel_StreamAsChannelAsync()
        {
            channel = Channel.CreateUnbounded<int>();
            cts = new CancellationTokenSource();
            for (int i = 0; i < n; i++)
            {
                var _ = await conn.StreamAsChannelAsync<int>(nameof(IMockHub.GetChannelWithToken), cts.Token);
            }
            cts.Cancel();
        }
        
        [Benchmark]
        public async Task GetChannel_Strong()
        {
            channel = Channel.CreateUnbounded<int>();
            cts = new CancellationTokenSource();
            for (int i = 0; i < n; i++)
            {
                var _ = await hub.GetChannelWithToken(cts.Token);
            }
            cts.Cancel();
        }
        
        [Benchmark]
        public async Task GetChannel_Expr()
        {
            channel = Channel.CreateUnbounded<int>();
            cts = new CancellationTokenSource();
            for (int i = 0; i < n; i++)
            {
                var _ = await ehub.StreamAsChannelAsync(
                    h => h.GetChannelWithToken(cts.Token));
            }
            cts.Cancel();
        }
        
        [Benchmark]
        public async Task SetChannel_SendAsync()
        {
            channel = Channel.CreateUnbounded<int>();
            for (int i = 0; i < n; i++)
            {
                await conn.SendAsync(nameof(IMockHub.SetChannel), channel.Reader);
            }

            channel.Writer.TryComplete();
        }
        
        [Benchmark]
        public async Task SetChannel_Strong()
        {
            channel = Channel.CreateUnbounded<int>();
            for (int i = 0; i < n; i++)
            {
                await hub.SetChannel(channel.Reader);
            }
            
            channel.Writer.TryComplete();
        }
        
        [Benchmark]
        public async Task SetChannel_Expr()
        {
            channel = Channel.CreateUnbounded<int>();
            for (int i = 0; i < n; i++)
            {
                await ehub.SendAsync(h => h.SetChannel(channel.Reader));
            }
            
            channel.Writer.TryComplete();
        }
    }
}