using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong;

namespace SignalR.Strong.Samples.SourceGenerator
{
    public interface IMockHub
    {
        Task Do(int a);

        void DoThat(DateTime b);

        Task<ChannelReader<int>> Stream(CancellationToken token);

        IAsyncEnumerable<int> Stream2();

        Task<float> ReturnGeneric();

        Task ReverseStream(ChannelReader<int> c);

        Task ReverseStream(IAsyncEnumerable<float> f);

        Task Huh();
    }
    
    class Program
    {
        static async Task Main(string[] args)
        {
            var conn = new HubConnectionBuilder().WithUrl("http://localhost/").Build();
            var hub = conn.AsGeneratedHub<IMockHub>();
            await hub.Do(1);
        }
    }
}