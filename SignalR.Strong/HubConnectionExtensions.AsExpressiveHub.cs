using Microsoft.AspNetCore.SignalR.Client;

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