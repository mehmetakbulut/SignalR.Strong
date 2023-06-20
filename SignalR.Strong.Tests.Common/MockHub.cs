using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalR.Strong.Samples.Common.Hubs;

namespace SignalR.Strong.Tests.Common
{
    public class MockHub : Hub, IMockHub
    {
        public Task<int> GetValueType()
        {
            return Task.FromResult(123);
        }

        public Task GetVoid()
        {
            return Task.CompletedTask;
        }

        public Task<string> GetReferenceType()
        {
            return Task.FromResult("abc");
        }

        public Task<List<int>> GetCollection()
        {
            return Task.FromResult(new List<int>() {1, 2, 3});
        }

        public Task<int> SetValueType(int a)
        {
            return Task.FromResult(a);
        }

        public Task<string> SetReferenceType(string a)
        {
            return Task.FromResult(a);
        }

        public Task<List<int>> SetCollection(List<int> a)
        {
            return Task.FromResult(a);
        }

        public Task<ChannelReader<int>> StreamToClientViaChannel(List<int> a)
        {
            var channel = Channel.CreateUnbounded<int>();

            async Task WriteItemsAsync(ChannelWriter<int> writer, List<int> list)
            {
                Exception localException = null;
                try
                {
                    foreach (var item in list)
                    {
                        await writer.WriteAsync(item);
                    }
                }
                catch (Exception ex)
                {
                    localException = ex;
                }
                finally
                {
                    writer.Complete(localException);
                }
            }

            _ = WriteItemsAsync(channel.Writer, a);

            return Task.FromResult(channel.Reader);
        }

        public Task<ChannelReader<int>> StreamToClientViaChannelWithToken(List<int> a, CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<int>();
            
            async Task WriteItemsAsync(ChannelWriter<int> writer, List<int> list, CancellationToken tok)
            {
                Exception localException = null;
                try
                {
                    foreach (var item in list)
                    {
                        tok.ThrowIfCancellationRequested();
                        await writer.WriteAsync(item);
                        await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    localException = ex;
                }
                finally
                {
                    writer.Complete(localException);
                }
            }

            _ = WriteItemsAsync(channel.Writer, a, cancellationToken);

            return Task.FromResult(channel.Reader);
        }

        public async Task StreamFromClientViaChannel(List<int> a, ChannelReader<int> reader)
        {
            var channel = Channel.CreateUnbounded<int>();

            await channel.Reader.ReadAsync();

            return;
        }
        
        public async IAsyncEnumerable<int> StreamToClientViaEnumerableWithToken(
            List<int> a,
            [EnumeratorCancellation]
            CancellationToken cancellationToken)
        {
            // adapted from https://docs.microsoft.com/en-us/aspnet/core/signalr/streaming?view=aspnetcore-5.0
            foreach (var i in a)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return i;

                await Task.Yield();
            }
        }
        
        public async Task StreamFromClientViaEnumerable(List<int> a, IAsyncEnumerable<int> reader)
        {
            // adapted from https://docs.microsoft.com/en-us/aspnet/core/signalr/streaming?view=aspnetcore-5.0
            await foreach (var item in reader)
            {
                await Task.Yield();
            }
        }
        
        /*private ChannelReader<int> _loopInput
        {
            get => (ChannelReader<int>)Context.Items["loopInput"];
            set => Context.Items["loopInput"] = value;
        }
        private Channel<int> _loopOutput
        {
            get => (Channel<int>)Context.Items["loopOutput"];
            set => Context.Items["loopOutput"] = value;
        }
        private CancellationToken _loopToken
        {
            get => (CancellationToken)Context.Items["loopToken"];
            set => Context.Items["loopToken"] = value;
        }
        
        public async Task<ChannelReader<int>> LoopRx()
        {
            while (!Context.Items.ContainsKey("loop"))
            {
                await Task.Delay(1);
            }

            return _loopOutput.Reader;
        }

        public async Task LoopTx(ChannelReader<int> reader)
        {
            _loopInput = reader;
            _loopToken = default;
            _loopOutput = Channel.CreateUnbounded<int>();
            Context.Items["loop"] = true;

            await loopItemsAsync();
        }

        public Task<bool> LoopReset()
        {
            Context.Items.Remove("loopInput");
            Context.Items.Remove("loopOutput");
            Context.Items.Remove("loopToken");
            Context.Items.Remove("loop");
            return Task.FromResult(true);
        }

        private async Task loopItemsAsync()
        {
            Exception exception = null;
            try
            {
                while (await _loopInput.WaitToReadAsync(_loopToken))
                {
                    while (_loopInput.TryRead(out var item))
                    {
                        await _loopOutput.Writer.WriteAsync(item, _loopToken);
                    }
                }

                exception = default;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                _loopOutput.Writer.TryComplete(exception);
            }
        }*/

        public async Task TriggerSyncCallback()
        {
            await Clients.Caller.SendAsync("ReceiveSyncCallback");
        }
        
        public async Task TriggerAsyncCallback()
        {
            await Clients.Caller.SendAsync("ReceiveAsyncCallback");
        }
        
        public Task<ChannelReader<int>> GetRxChannel()
        {
            var channel = Channel.CreateUnbounded<int>();

            return Task.FromResult(channel.Reader);
        }

        public Task<ChannelReader<int>> GetChannelWithToken(CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<int>();

            cancellationToken.Register(() => channel.Writer.TryComplete());

            return Task.FromResult(channel.Reader);
        }

        public Task SetChannel(ChannelReader<int> reader)
        {
            return Task.CompletedTask;
        }

        public Task<string> Hello2(string name)
        {
            return Task.FromResult($"[2] Hello, {name}!");
        }

        public Task<string> Hello1(string name)
        {
            return Task.FromResult($"[1] Hello, {name}!");
        }
    }
}