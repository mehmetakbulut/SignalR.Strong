using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong.Tests.Common;

namespace SignalR.Strong.Tests.Benchmark
{
    [MemoryDiagnoser]
    public class Rpc
    {
        public ServerFixture fixture;
        public StrongClient client;
        public IMockHub hub;
        public HubConnection conn;
        private Channel<int> channel;

        [GlobalSetup]
        public async Task SetupAsync()
        {
            fixture = new ServerFixture();
            client = await fixture.GetClient();
            hub = client.GetHub<IMockHub>();
            conn = client.GetConnection<IMockHub>();
            channel = Channel.CreateUnbounded<int>();
        }
        
        [Benchmark]
        public async Task GetVoid_SendAsync()
        {
            await conn.SendAsync("GetVoid");
        }
        
        [Benchmark]
        public async Task GetVoid_InvokeAsync()
        {
            await conn.InvokeAsync("GetVoid");
        }
        
        [Benchmark]
        public async Task GetVoid_Strong()
        {
            await hub.GetVoid();
        }
        
        [Benchmark]
        public async Task GetValueType_InvokeAsync()
        {
            var _ = await conn.InvokeAsync<int>("GetValueType");
        }
        
        [Benchmark]
        public async Task GetValueType_Strong()
        {
            var _ = await hub.GetValueType();
        }
        
        [Benchmark]
        public async Task<int> SetValueType_InvokeAsync()
        {
            return await conn.InvokeAsync<int>("SetValueType", 123);
        }
        
        [Benchmark]
        public async Task<int> SetValueType_Strong()
        {
            return await hub.SetValueType(123);
        }
    }
}
