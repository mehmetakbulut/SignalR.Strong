using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong.Expressive;

namespace SignalR.Strong
{
    public static partial class HubConnectionExtensionsExpressive
    {
        public static ExpressiveHub<THub> AsExpressiveHub<THub>(this HubConnection conn)
        {
            return new ExpressiveHub<THub>(conn);
        }
    }
}