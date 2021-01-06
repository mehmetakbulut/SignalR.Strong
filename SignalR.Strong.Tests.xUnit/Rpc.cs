using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Collections;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong.Samples.Common.Hubs;
using SignalR.Strong.Tests.Common;
using Xunit;

namespace SignalR.Strong.Tests.xUnit
{
    public class Rpc : IClassFixture<ServerFixture>
    {
        private ServerFixture fixture;

        public Rpc(ServerFixture fixture)
        {
            this.fixture = fixture;
        }
        
        [Fact]
        public async Task Get()
        {
            var conn = await fixture.GetHubConnection();
            var hub = conn.AsDynamicHub<IMockHub>();
            
            await hub.GetVoid();

            Assert.Equal(123, await hub.GetValueType());

            Assert.Equal("abc", await hub.GetReferenceType());

            var col = await hub.GetCollection();
            col.Should().BeEquivalentTo(new List<int>() {1, 2, 3});
        }

        [Fact]
        public async Task Set()
        {
            var conn = await fixture.GetHubConnection();
            var hub = conn.AsDynamicHub<IMockHub>();
            
            Assert.Equal(123, await hub.SetValueType(123));

            Assert.Equal("abc", await hub.SetReferenceType("abc"));

            var col = await hub.SetCollection(new List<int>() {1, 2, 3});
            col.Should().BeEquivalentTo(new List<int>() {1, 2, 3});
        }
        
        [Fact]
        public async Task GetViaExpr()
        {
            var conn = await fixture.GetHubConnection();
            var ehub = conn.AsExpressiveHub<IMockHub>();

            await ehub.SendAsync(hub => hub.GetVoid());
            await ehub.InvokeAsync(hub => hub.GetVoid());

            Assert.Equal(123, await ehub.InvokeAsync(hub => hub.GetValueType()));

            Assert.Equal("abc", await ehub.InvokeAsync(hub => hub.GetReferenceType()));

            var col = await ehub.InvokeAsync(hub => hub.GetCollection());
            col.Should().BeEquivalentTo(new List<int>() {1, 2, 3});
        }

        [Fact]
        public async Task SetViaExpr()
        {
            var conn = await fixture.GetHubConnection();
            var ehub = conn.AsExpressiveHub<IMockHub>();
            
            Assert.Equal(123, await ehub.InvokeAsync(hub => hub.SetValueType(123)));

            Assert.Equal("abc", await ehub.InvokeAsync(hub => hub.SetReferenceType("abc")));

            var col = await ehub.InvokeAsync(hub => hub.SetCollection(new List<int>() {1, 2, 3}));
            col.Should().BeEquivalentTo(new List<int>() {1, 2, 3});
        }
    }
}