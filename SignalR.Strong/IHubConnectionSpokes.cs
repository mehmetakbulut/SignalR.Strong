using System;
using Microsoft.AspNetCore.SignalR.Client;
// ReSharper disable UnusedMember.Global

namespace SignalR.Strong
{
    /// <summary>
    /// Spokes on the <see cref="HubConnection"/> interface
    /// </summary>
    public interface IHubConnectionSpokes
        : IDisposable
    {
        /// <summary>
        /// Configure dependency-injected Spoke to handle HubConnection events
        /// </summary>
        /// <param name="connection"><see cref="HubConnection"/></param>
        /// <typeparam name="TSpoke">Spoke type to be used</typeparam>
        /// <returns>Returns this to chain calls</returns>
        /// <exception cref="ArgumentNullException">Throws if <see cref="HubConnection"/> connection is null</exception>
        IHubConnectionSpokes AddSpoke<TSpoke>(HubConnection connection);
    }
}