using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SignalR.Strong.Tests.Common;
using Xunit;

namespace SignalR.Strong.Tests.xUnit
{
    public class Spokes : IClassFixture<ServerFixture>
    {
        private ServerFixture fixture;

        public Spokes(ServerFixture fixture)
        {
            this.fixture = fixture;
        }
        
        private interface IMockSpoke
        {
            void ReceiveSyncCallback();

            Task ReceiveAsyncCallback();
        }
        
        private class MockSpoke : Spoke<IMockSpoke>, IMockSpoke
        {
            public void ReceiveSyncCallback()
            {
                GotSyncCallback = true;
            }
            
            public bool GotSyncCallback { get; private set; }
            
            public Task ReceiveAsyncCallback()
            {
                GotAsyncCallback = true;
                return Task.CompletedTask;
            }
            
            public bool GotAsyncCallback { get; private set; }
        }
        
        [Fact]
        public async Task ReceivesCallback()
        {
            var conn = new HubConnectionBuilder()
                .WithUrl(fixture.GetCompleteServerUrl("/mockhub"))
                .Build();
            var client = new StrongClient();
            client.RegisterHub<IMockHub>(conn);
            client.RegisterSpoke<MockSpoke, IMockHub>();
            await client.ConnectToHubsAsync();
            client.Build();

            var spoke = client.GetSpoke<MockSpoke>();

            spoke.GotSyncCallback.Should().BeFalse();
            spoke.GotAsyncCallback.Should().BeFalse();

            await client.GetHub<IMockHub>().TriggerSyncCallback();
            await Task.Delay(1000);
            spoke.GotSyncCallback.Should().BeTrue();
            
            await client.GetHub<IMockHub>().TriggerAsyncCallback();
            await Task.Delay(1000);
            spoke.GotAsyncCallback.Should().BeTrue();
        }

        private interface IDependency
        {
            
        }

        public class Dependency : IDependency
        {
            
        }
        
        private interface IDependentSpoke
        {
        }
        
        private class DependentSpoke : Spoke<IDependentSpoke>, IDependentSpoke
        {
            public IDependency Dependency { get; private set; }
            
            public DependentSpoke(IDependency dependency)
            {
                this.Dependency = dependency;
            }
        }

        [Fact]
        public void HasDependencies()
        {
            var conn = new HubConnectionBuilder()
                .WithUrl(fixture.GetCompleteServerUrl("/mockhub"))
                .Build();
            var services = new ServiceCollection();
            services.AddSingleton<IDependency, Dependency>();
            var provider = services.BuildServiceProvider();
            var client = new StrongClient(provider);
            client.RegisterHub<IMockHub>(conn);
            client.RegisterSpoke<DependentSpoke, IMockHub>();
            client.Build();

            var spoke = client.GetSpoke<DependentSpoke>();
            spoke.Dependency.Should().NotBeNull();
            spoke.Dependency.Should().BeSameAs(provider.GetRequiredService<IDependency>());
        }
    }
}