// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Project
{
    // API in line with Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions class.

    /// <summary>
    /// Provides extensions methods to the <see cref="IServiceProvider"/> interface.
    /// </summary>
    /// <seealso cref="IService"/>
    [CompileTime]
    public static class ServiceProviderExtensions
    {
        public static T GetRequiredService<T>( this IServiceProvider<IService> serviceProvider )
            where T : class, IService
        {
            var service = (T?) serviceProvider.GetService( typeof(T) );

            if ( service == null )
            {
                throw new InvalidOperationException( $"Cannot get the service {typeof(T).Name}." );
            }

            return service;
        }

        public static T GetRequiredService<T>( this IServiceProvider<IProjectService> serviceProvider )
            where T : class, IProjectService
        {
            var service = (T?) serviceProvider.GetService( typeof(T) );

            if ( service == null )
            {
                throw new InvalidOperationException( $"Cannot get the service {typeof(T).Name}." );
            }

            return service;
        }
    }
}