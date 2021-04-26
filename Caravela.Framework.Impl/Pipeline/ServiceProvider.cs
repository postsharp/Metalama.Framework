// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Pipeline
{
    public class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        public void AddService<T>( T service )
            where T : notnull
            => this._services.Add( typeof(T), service );

        public object? GetService( Type serviceType )
        {
            _ = this._services.TryGetValue( serviceType, out var instance );

            return instance;
        }
    }
}