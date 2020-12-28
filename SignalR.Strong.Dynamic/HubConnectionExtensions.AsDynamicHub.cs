using System;
using System.CodeDom.Compiler;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong.Dynamic;

namespace SignalR.Strong
{
    public static partial class HubConnectionExtensionsDynamic
    {
        private static ProxyGenerator proxyGenerator = new ProxyGenerator();  
        
        public static THub AsDynamicHub<THub>(this HubConnection conn)
        {
            return (THub) conn.AsDynamicHub(typeof(THub));
        }

        public static object AsDynamicHub(this HubConnection conn, System.Type hubType)
        {
            var hubInterceptor = new HubInterceptor(conn);
            var hubProxy = proxyGenerator.CreateInterfaceProxyWithoutTarget(hubType, hubInterceptor.ToInterceptor());
            return hubProxy;
        }
    }
}