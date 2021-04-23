// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl
{
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>( this IServiceProvider serviceProvider )
            where T : class
        {
            var service = (T?) serviceProvider.GetService( typeof(T) );

            if ( service == null )
            {
                throw new AssertionFailedException( $"Cannot get the service {typeof(T).Name}." );
            }

            return service;
        }
    }
}