using System;

namespace Caravela.Framework.Impl
{
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>( this IServiceProvider serviceProvider )
        {
            var service = (T?) serviceProvider.GetService( typeof(T) );
            if ( service == null )
            {
                throw new AssertionFailedException();
            }

            return service;
        }
        
        
    }
}