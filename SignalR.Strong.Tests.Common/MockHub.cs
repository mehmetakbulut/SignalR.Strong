using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalR.Strong.Tests.Common
{
    public interface IMockHub
    {
        Task<int> GetValueType();

        Task GetVoid();

        Task<string> GetReferenceType();

        Task<List<int>> GetCollection();

        Task<int> SetValueType(int a);

        Task<string> SetReferenceType(string a);

        Task<List<int>> SetCollection(List<int> a);

        Task<ChannelReader<int>> StreamToClient(List<int> a);
        
        Task<ChannelReader<int>> StreamToClientWithToken(List<int> a, CancellationToken cancellationToken);

        Task StreamFromClient(List<int> a, ChannelReader<int> reader);
        
        Task<ChannelReader<int>> LoopRx();

        Task LoopTx(ChannelReader<int> reader);

        Task<bool> LoopReset();
        
        Task TriggerSyncCallback();
        
        Task TriggerAsyncCallback();
        Task<ChannelReader<int>> GetRxChannel();
        Task<ChannelReader<int>> GetRxChannelWithToken(CancellationToken cancellationToken);
        Task GetTxChannel(ChannelReader<int> reader);
    }

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

        public Task<ChannelReader<int>> StreamToClient(List<int> a)
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

        public Task<ChannelReader<int>> StreamToClientWithToken(List<int> a, CancellationToken cancellationToken)
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

        public async Task StreamFromClient(List<int> a, ChannelReader<int> reader)
        {
            var channel = Channel.CreateUnbounded<int>();

            await channel.Reader.ReadAsync();

            return;
        }
        
        private ChannelReader<int> _loopInput
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
        }

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

        public Task<ChannelReader<int>> GetRxChannelWithToken(CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<int>();

            cancellationToken.Register(() => channel.Writer.TryComplete());

            return Task.FromResult(channel.Reader);
        }

        public Task GetTxChannel(ChannelReader<int> reader)
        {
            return Task.CompletedTask;
        }
    }
}