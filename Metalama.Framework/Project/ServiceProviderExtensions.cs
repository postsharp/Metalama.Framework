// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Project
{
    // API in line with Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions class.

    /// <summary>
    /// Provides extensions methods to the <see cref="IServiceProvider"/> interface.
    /// </summary>
    /// <seealso cref="IService"/>
    [CompileTimeOnly]
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Gets a service or throws an <see cref="InvalidOperationException"/> if the requested service has not been registered.
        /// </summary>
        public static T GetRequiredService<T>( this IServiceProvider serviceProvider )
            where T : class, IService
        {
            var service = (T?) serviceProvider.GetService( typeof(T) );

            if ( service == null )
            {
                throw new InvalidOperationException( $"Cannot get the service {typeof(T).Name}." );
            }

            return service;
        }

        /// <summary>
        /// Gets a service or returns <c>null</c> if the requested service has not been registered.
        /// </summary>
        public static T? GetService<T>( this IServiceProvider serviceProvider )
            where T : class
            => (T?) serviceProvider.GetService( typeof(T) );
    }
}