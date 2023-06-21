using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace SignalR.Strong
{
    public static class HubConnectionExtensionsSpokes
    {
        public static SpokeRegistration RegisterSpoke(this HubConnection conn, object spoke, Type intfType = null)
        {
            var regs = new List<IDisposable>(); 
            
            if (spoke is ISpoke ispoke)
            {
                // If spoke implements the spoke interface, then set its properties
                ispoke.Connection = conn;
            }

            if (intfType is null)
            {
                intfType = spoke.GetType();
            }
            
            var methods = intfType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods)
            {
                var reg = conn.On(method.Name, method.GetParameters().Select(a => a.ParameterType).ToArray(),
                    objects =>
                    {
                        method.Invoke(spoke, objects);
                        return Task.CompletedTask;
                    });
                regs.Add(reg);
            }

            return new SpokeRegistration(spoke, regs);
        }
        
        public static SpokeRegistration RegisterSpoke<TSpokeIntf, TSpokeImpl>(this HubConnection conn, TSpokeImpl spoke)
            where TSpokeImpl : TSpokeIntf
        {
            return conn.RegisterSpoke(spoke, typeof(TSpokeIntf));
        }
        
        public static SpokeRegistration RegisterSpoke<TSpoke>(this HubConnection conn, TSpoke spoke)
        {
            return conn.RegisterSpoke<TSpoke, TSpoke>(spoke);
        }

        public static SpokeRegistration RegisterSpoke<TSpokeIntf>(this HubConnection conn, object spoke)
        {
            return conn.RegisterSpoke(spoke, typeof(TSpokeIntf));
        }

        public static SpokeRegistration RegisterSpoke(this HubConnection conn, System.Type intfType, System.Type implType, IServiceProvider provider = null)
        {
            object spoke;
            if (provider is null)
            {
                spoke = Activator.CreateInstance(implType);
            }
            else
            {
                // Perform dependency injection if a service provider is given
                spoke = ActivatorUtilities.CreateInstance(provider, intfType);
            }

            return conn.RegisterSpoke(spoke, intfType);
        }
        
        public static SpokeRegistration RegisterSpoke<TSpokeIntf, TSpokeImpl>(this HubConnection conn, IServiceProvider provider = null)
            where TSpokeImpl : TSpokeIntf
        {
            return conn.RegisterSpoke(typeof(TSpokeIntf), typeof(TSpokeImpl), provider);
        }
        
        public static SpokeRegistration RegisterSpoke<TSpoke>(this HubConnection conn, IServiceProvider provider = null)
        {
            return conn.RegisterSpoke<TSpoke, TSpoke>(provider);
        }
    }
}