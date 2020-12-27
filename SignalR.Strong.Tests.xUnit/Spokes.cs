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
            var registration = conn.RegisterSpoke<MockSpoke>();
            await conn.StartAsync();

            var spoke = (MockSpoke) registration.Spoke;

            spoke.GotSyncCallback.Should().BeFalse();
            spoke.GotAsyncCallback.Should().BeFalse();

            await conn.AsDynamicHub<IMockHub>().TriggerSyncCallback();
            await Task.Delay(1000);
            spoke.GotSyncCallback.Should().BeTrue();
            
            await conn.AsDynamicHub<IMockHub>().TriggerAsyncCallback();
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
            var reg = conn.RegisterSpoke<DependentSpoke>(provider);

            var spoke = (DependentSpoke) reg.Spoke;
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
            var conn = new HubConnectionBuilder().WithUrl("http://localhost/").Build();
            
            var reg = conn.RegisterSpoke<ISpokeInterface, SpokeImplementation>();
            var spoke = reg.Spoke;
            
            reg = conn.RegisterSpoke<SpokeImplementation>();
            spoke = reg.Spoke;
            
            reg = conn.RegisterSpoke<ISpokeInterface, SpokeImplementation>((SpokeImplementation)spoke);
            spoke = (ISpokeInterface) reg.Spoke;
        }
        
        private class AutoFedSpoke : ISpoke
        {
            public HubConnection Connection { get; set; }
        }
        
        [Fact]
        public void AutoFedSpokeHasPropertiesSet()
        {
            var conn = new HubConnectionBuilder().WithUrl("http://localhost/").Build();
            var reg = conn.RegisterSpoke<AutoFedSpoke>();
            var spoke = (AutoFedSpoke) reg.Spoke;
            spoke.Connection.Should().BeSameAs(conn);
        }
    }
}