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
        
        private class MockSpoke
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
            client.BuildSpokes();

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
        
        private class DependentSpoke
        {
            public IDependency Dependency { get; private set; }
            
            public DependentSpoke(IDependency dependency)
            {
                this.Dependency = dependency;
            }

            public void SomeMethod()
            {
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
            client.BuildSpokes();

            var spoke = client.GetSpoke<DependentSpoke>();
            spoke.Dependency.Should().NotBeNull();
            spoke.Dependency.Should().BeSameAs(provider.GetRequiredService<IDependency>());
        }

        private interface ISpokeInterface
        {
            void DoSomething();
        }

        private class SpokeImplementation : ISpokeInterface
        {
            public void DoSomething()
            {
                
            }
        }

        [Fact]
        public void RegisterAndGetSpoke()
        {
            var client = new StrongClient();
            client.RegisterHub<IMockHub>(new HubConnectionBuilder().WithUrl("http://localhost/").Build());
            client.RegisterSpoke<ISpokeInterface, SpokeImplementation, IMockHub>();
            client.BuildSpokes();
            var spoke = client.GetSpoke<ISpokeInterface>();
            
            client = new StrongClient();
            client.RegisterHub<IMockHub>(new HubConnectionBuilder().WithUrl("http://localhost/").Build());
            client.RegisterSpoke<SpokeImplementation, IMockHub>();
            client.BuildSpokes();
            spoke = client.GetSpoke<SpokeImplementation>();
            
            client = new StrongClient();
            client.RegisterHub<IMockHub>(new HubConnectionBuilder().WithUrl("http://localhost/").Build());
            client.RegisterSpoke<ISpokeInterface, SpokeImplementation, IMockHub>((SpokeImplementation)spoke);
            client.BuildSpokes();
            spoke = (ISpokeInterface) client.GetSpoke(typeof(ISpokeInterface));
        }
        
        private class AutoFedSpoke : Spoke<IMockHub>
        {
            public override HubConnection Connection { get; set; }
            public override StrongClient Client { get; set; }
            public override object WeakHub { get; set; }
        }
        
        [Fact]
        public async Task AutoFedSpokeHasPropertiesSet()
        {
            var client = new StrongClient();
            client.RegisterHub<IMockHub>(new HubConnectionBuilder().WithUrl("http://localhost/").Build());
            client.RegisterSpoke<AutoFedSpoke, IMockHub>();
            client.BuildSpokes();
            var spoke = client.GetSpoke<AutoFedSpoke>();
            spoke.Client.Should().BeSameAs(client);
            var conn = client.GetConnection<IMockHub>();
            spoke.Connection.Should().BeSameAs(conn);
            spoke.Connection.Should().BeSameAs(conn);
            var hub = client.GetHub<IMockHub>();
            spoke.WeakHub.Should().BeSameAs(hub);
            spoke.Hub.Should().BeSameAs(hub);
        }
    }
}