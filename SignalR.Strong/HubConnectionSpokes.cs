using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong.IoC;

// ReSharper disable HeapView.PossibleBoxingAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable UnusedType.Global
// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedMember.Global
// ReSharper disable InvertIf

namespace SignalR.Strong
{
    /// <inheritdoc/>
    public sealed class HubConnectionSpokes
        : IHubConnectionSpokes
    {
        private readonly ConcurrentBag<IDisposable> _disconnectors = new ConcurrentBag<IDisposable>();
        private readonly IServiceProvider _services;

        /// <summary>
        /// Constructs registration container that manages <see cref="HubConnection"/>-connected
        /// spokes (<see cref="HubConnection"/> incoming event handlers).
        /// </summary>
        /// <param name="services">IoC <see cref="IServiceProvider"/></param>
        /// <exception cref="ArgumentNullException">
        /// Throws is <see cref="IServiceProvider"/> services is null
        /// </exception>
        public HubConnectionSpokes(IServiceProvider services)
            => _services = services ?? throw new ArgumentNullException(nameof(services));

        /// <inheritdoc/>
        public IHubConnectionSpokes AddSpoke<TSpoke>(HubConnection connection)
        {
            var methods = typeof(TSpoke)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public);

            foreach (var method in methods)
            {
                connection.ListenWith<TSpoke>(_services, method, _disconnectors.Add);
            }

            return this;
        }

        /// <summary>
        /// Dispose and clean up all registrations
        /// </summary>
        public void Dispose()
        {
            while (_disconnectors.TryTake(out var disconnector))
            {
                disconnector.Dispose();
            }
        }
    }
}