using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Pipeline
{
    internal class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void AddService<T>( T service ) => this._services.Add( typeof(T), service );
        

        public object? GetService( Type serviceType )
        {
            _ = this._services.TryGetValue( serviceType, out var instance );
            return instance;
        }
    }
}