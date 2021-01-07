using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong.Expressive;
using SignalR.Strong.Samples.Common.Hubs;
using SignalR.Strong.Tests.Common;

namespace SignalR.Strong.Tests.Benchmark
{
    [MemoryDiagnoser]
    public class Rpc
    {
        public ServerFixture fixture;
        public IMockHub hub;
        public ExpressiveHub<IMockHub> ehub;
        public HubConnection conn;
        private Channel<int> channel;

        [GlobalSetup]
        public async Task SetupAsync()
        {
            fixture = new ServerFixture();
            conn = await fixture.GetHubConnection();
            hub = conn.AsDynamicHub<IMockHub>();
            ehub = conn.AsExpressiveHub<IMockHub>();
            channel = Channel.CreateUnbounded<int>();
        }
        
        [Benchmark]
        public async Task GetVoid_SendAsync()
        {
            await conn.SendAsync(nameof(IMockHub.GetVoid));
        }
        
        [Benchmark]
        public async Task GetVoid_InvokeAsync()
        {
            await conn.InvokeAsync(nameof(IMockHub.GetVoid));
        }
        
        [Benchmark]
        public async Task GetVoid_Strong()
        {
            await hub.GetVoid();
        }
        
        [Benchmark]
        public async Task GetVoid_ExprSendAsync()
        {
            await ehub.SendAsync(h => h.GetVoid());
        }
        
        [Benchmark]
        public async Task GetVoid_ExprInvokeAsync()
        {
            await ehub.InvokeAsync(h => h.GetVoid());
        }
        
        [Benchmark]
        public async Task GetValueType_InvokeAsync()
        {
            var _ = await conn.InvokeAsync<int>(nameof(IMockHub.GetValueType));
        }
        
        [Benchmark]
        public async Task GetValueType_Strong()
        {
            var _ = await hub.GetValueType();
        }
        
        [Benchmark]
        public async Task GetValueType_Expr()
        {
            var _ = await ehub.InvokeAsync(h => h.GetValueType());
        }

        [Benchmark]
        public async Task<int> SetValueType_InvokeAsync()
        {
            return await conn.InvokeAsync<int>(nameof(IMockHub.SetValueType), 123);
        }
        
        [Benchmark]
        public async Task<int> SetValueType_Strong()
        {
            return await hub.SetValueType(123);
        }
        
        [Benchmark]
        public async Task<int> SetValueType_Expr()
        {
            return await ehub.InvokeAsync(h => h.SetValueType(123));
        }
    }
}
