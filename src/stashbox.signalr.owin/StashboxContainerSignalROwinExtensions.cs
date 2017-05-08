﻿using System;
using System.Reflection;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Stashbox.AspNet.SignalR;
using Stashbox.Utils;

namespace Stashbox.Infrastructure
{
    public static class StashboxContainerSignalROwinExtensions
    {
        /// <summary>
        /// Adds <see cref="StashboxContainer"/> as the default dependency resolver and the default <see cref="IHubActivator"/>, also registers the available <see cref="IHub"/> and <see cref="PersistentConnection"/> implementations.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="config">The hub configuration.</param>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>The container.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="container"/> or <paramref name="config"/> is <c>null</c>.
        /// </exception>
        public static IStashboxContainer AddOwinSignalR(this IStashboxContainer container, HubConfiguration config, params Assembly[] assemblies)
        {
            Shield.EnsureNotNull(container, nameof(container));
            Shield.EnsureNotNull(config, nameof(config));

            container.RegisterInstance<Microsoft.AspNet.SignalR.IDependencyResolver>(new StashboxDependencyResolver(container));
            container.RegisterInstance<IHubActivator>(new StashboxHubActivator(container));
            config.Resolver = container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>();

            return container.RegisterHubs(assemblies).RegisterPersistentConnections(assemblies);
        }

        /// <summary>
        /// Adds <see cref="StashboxContainer"/> as the default dependency resolver and the default <see cref="IHubActivator"/>, also registers the available <see cref="IHub"/> and <see cref="PersistentConnection"/> implementations.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="config">The hub configuration.</param>
        /// <param name="types">The types.</param>
        /// <returns>The container.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="container"/> or <paramref name="config"/> is <c>null</c>.
        /// </exception>
        public static IStashboxContainer AddOwinSignalRWithTypes(this IStashboxContainer container, HubConfiguration config, params Type[] types)
        {
            Shield.EnsureNotNull(container, nameof(container));
            Shield.EnsureNotNull(config, nameof(config));

            container.RegisterInstance<Microsoft.AspNet.SignalR.IDependencyResolver>(new StashboxDependencyResolver(container));
            container.RegisterInstance<IHubActivator>(new StashboxHubActivator(container));
            config.Resolver = container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>();

            return container.RegisterHubs(types).RegisterPersistentConnections(types);
        }
    }
}
