using System;
using System.CodeDom.Compiler;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Strong
{
    public static partial class HubConnectionExtensions
    {
        public static THub AsDynamicProxy<THub>(this HubConnection conn)
        {
            return (THub) conn.AsDynamicProxy(typeof(THub));
        }

        public static object AsDynamicProxy(this HubConnection conn, System.Type hubType)
        {
            var generator = new ProxyGenerator();
            var hubInterceptor = new HubInterceptor(conn);
            var hubProxy = generator.CreateInterfaceProxyWithoutTarget(hubType, hubInterceptor.ToInterceptor());
            return hubProxy;
        }
    }
}