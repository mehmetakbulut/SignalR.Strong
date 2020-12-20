using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Strong
{
    public interface ISpoke
    {
        HubConnection Connection { get; set; }
        StrongClient Client { get; set; }
        object WeakHub { get; set; }
    }
    
    public abstract class Spoke<THub> : ISpoke
    {
        public virtual THub Hub => (THub) WeakHub;
        public abstract HubConnection Connection { get; set; }
        public abstract StrongClient Client { get; set; }
        public abstract object WeakHub { get; set; }
    }
}