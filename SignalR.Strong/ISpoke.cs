using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Strong
{
    public interface ISpoke
    {
        HubConnection Connection { get; set; }
        StrongClient Client { get; set; }
    }
}