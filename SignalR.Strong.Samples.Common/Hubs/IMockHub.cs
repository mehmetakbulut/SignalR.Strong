using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SignalR.Strong.Samples.Common.Hubs
{
    public interface IMockHub : IMockBase
    {
        Task<int> GetValueType();

        Task GetVoid();

        Task<string> GetReferenceType();

        Task<List<int>> GetCollection();

        Task<int> SetValueType(int a);

        Task<string> SetReferenceType(string a);

        Task<List<int>> SetCollection(List<int> a);

        Task<ChannelReader<int>> StreamToClientViaChannel(List<int> a);

        Task<ChannelReader<int>> StreamToClientViaChannelWithToken(List<int> a, CancellationToken cancellationToken);

        Task StreamFromClientViaChannel(List<int> a, ChannelReader<int> reader);

        IAsyncEnumerable<int> StreamToClientViaEnumerableWithToken(
            List<int> a,
            [EnumeratorCancellation]
            CancellationToken cancellationToken);

        Task StreamFromClientViaEnumerable(List<int> a, IAsyncEnumerable<int> reader);

        Task TriggerSyncCallback();

        Task TriggerAsyncCallback();
        Task<ChannelReader<int>> GetRxChannel();
        Task<ChannelReader<int>> GetChannelWithToken(CancellationToken cancellationToken);
        Task SetChannel(ChannelReader<int> reader);
    }
}
