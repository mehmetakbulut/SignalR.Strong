using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong.Tests.Common;

namespace SignalR.Strong.Tests.Benchmark
{
    [MemoryDiagnoser]
    public class Stream
    {
        public ServerFixture fixture;
        public StrongClient client;
        public IMockHub hub;
        public HubConnection conn;
        private Channel<int> channel;
        private CancellationTokenSource cts;
        private int n = 1;

        [GlobalSetup]
        public async Task SetupAsync()
        {
            fixture = new ServerFixture();
            client = await fixture.GetClient();
            hub = client.GetHub<IMockHub>();
            conn = client.GetConnection<IMockHub>();
        }
        
        [Benchmark]
        public async Task GetRxChannel_StreamAsChannelAsync()
        {
            channel = Channel.CreateUnbounded<int>();
            cts = new CancellationTokenSource();
            for (int i = 0; i < n; i++)
            {
                var _ = await conn.StreamAsChannelAsync<int>("GetRxChannelWithToken", cts.Token);
            }
            cts.Cancel();
        }
        
        [Benchmark]
        public async Task GetRxChannel_Strong()
        {
            channel = Channel.CreateUnbounded<int>();
            cts = new CancellationTokenSource();
            for (int i = 0; i < n; i++)
            {
                var _ = await hub.GetRxChannelWithToken(cts.Token);
            }
            cts.Cancel();
        }
        
        [Benchmark]
        public async Task SetReader_StreamAsChannelAsync()
        {
            channel = Channel.CreateUnbounded<int>();
            for (int i = 0; i < n; i++)
            {
                await conn.SendAsync("GetTxChannel", channel.Reader);
            }

            channel.Writer.TryComplete();
        }
        
        [Benchmark]
        public async Task SetReader_Strong()
        {
            channel = Channel.CreateUnbounded<int>();
            for (int i = 0; i < n; i++)
            {
                await hub.GetTxChannel(channel.Reader);
            }
            
            channel.Writer.TryComplete();
        }
    }
}