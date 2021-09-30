// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.ServiceProvider
{
    public class ServiceProvider : IServiceProvider, IDisposable
    {
        private readonly Dictionary<Type, object> _services = new();
        private bool _frozen;

        public void AddService( IService service )
        {
            if ( this._frozen )
            {
                throw new InvalidOperationException();
            }

            var interfaces = service.GetType().GetInterfaces();

            foreach ( var interfaceType in interfaces )
            {
                if ( typeof(IService).IsAssignableFrom( interfaceType ) )
                {
                    this._services[interfaceType] = service;
                }
            }

            for ( var cursorType = service.GetType();
                  cursorType != null && typeof(IService).IsAssignableFrom( cursorType );
                  cursorType = cursorType.BaseType )
            {
                this._services[cursorType] = service;
            }
        }

        public object? GetService( Type serviceType )
        {
            _ = this._services.TryGetValue( serviceType, out var instance );

            return instance;
        }

        public ServiceProvider() { }

        public ServiceProvider( ServiceProvider prototype )
        {
            this._services = new Dictionary<Type, object>( prototype._services );
        }

        public void Freeze() => this._frozen = true;

        public void Dispose()
        {
            foreach ( var o in this._services.Values )
            {
                if ( o is IDisposable disposable )
                {
                    disposable.Dispose();
                }
            }
        }
    }
}