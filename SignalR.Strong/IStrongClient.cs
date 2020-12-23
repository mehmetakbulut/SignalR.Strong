using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Strong
{
    public interface IStrongClient
    {
        bool IsBuilt { get; }
        IStrongClient RegisterSpoke<TSpoke, THub>();

        IStrongClient RegisterSpoke<TSpokeIntf, TSpokeImpl, THub>()
            where TSpokeImpl : TSpokeIntf;

        IStrongClient RegisterSpoke<TSpokeIntf, TSpokeImpl, THub>(TSpokeImpl spoke)
            where TSpokeImpl : TSpokeIntf;

        IStrongClient RegisterHub<THub>(HubConnection hubConnection) where THub : class;
        IStrongClient RegisterHub(Type hubType, HubConnection hubConnection);
        THub GetHub<THub>();
        object GetHub(Type hubType);
        ExpressiveHub<THub> GetExpressiveHub<THub>();
        HubConnection GetConnection<THub>();
        HubConnection GetConnection(Type hubType);
        TSpoke GetSpoke<TSpoke>();
        object GetSpoke(Type spokeType);
        Task<IStrongClient> ConnectToHubsAsync();
        IStrongClient BuildSpokes();
    }
}