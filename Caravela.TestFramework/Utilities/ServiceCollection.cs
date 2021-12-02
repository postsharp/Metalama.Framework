// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.TestFramework.Utilities
{
    /// <summary>
    /// Collection of services exposing an <see cref="IServiceProvider"/>.
    /// </summary>
    internal sealed class ServiceCollection
    {
        private readonly Dictionary<Type, object> _services = new();

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCollection"/> class.
        /// </summary>
        public ServiceCollection()
        {
            this._serviceProvider = new ServiceProvider( this );
        }

        /// <summary>
        /// Adds a service instance to the collection.
        /// </summary>
        /// <param name="type">Service type.</param>
        /// <param name="instance">Service instance.</param>
        public void AddService( Type type, object instance )
        {
            this._services[type] = instance;
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> providing the services added to this collection.
        /// </summary>
        /// <returns>The <see cref="IServiceProvider"/> providing the services added to this collection.</returns>
        public IServiceProvider GetServiceProvider()
        {
            return this._serviceProvider;
        }

        /// <summary>
        /// An <see cref="IServiceProvider"/> providing services from a <see cref="ServiceCollection"/>.
        /// </summary>
        public sealed class ServiceProvider : IServiceProvider
        {
            private readonly ServiceCollection _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceProvider"/> class.
            /// </summary>
            /// <param name="parent">The <see cref="ServiceCollection"/> to provide services from.</param>
            public ServiceProvider( ServiceCollection parent )
            {
                this._parent = parent;
            }

            /// <summary>
            /// Gets a service instance of type <paramref name="serviceType"/>.
            /// </summary>
            /// <param name="serviceType">Service type.</param>
            /// <returns>
            /// The service instance of type <paramref name="serviceType"/>
            /// or <c>null</c> if the service is not contained int the collection.
            /// </returns>
            public object? GetService( Type serviceType ) => this._parent._services.TryGetValue( serviceType, out var service ) ? service : null;
        }
    }
}