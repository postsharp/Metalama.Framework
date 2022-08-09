// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Framework.Engine.Utilities.Diagnostics
{
    public static class BackstageServiceFactoryInitializer
    {
        private static IApplicationInfo? _applicationInfo;

        public static bool IsInitialized => _applicationInfo != null;

        public static void Initialize<T>()
            where T : IApplicationInfo, new()
        {
            if ( _applicationInfo != null )
            {
                if ( _applicationInfo.GetType() != typeof(T) )
                {
                    throw new InvalidOperationException( "The services were initialized with a different implementation of IApplicationInfo." );
                }
                else
                {
                    return;
                }
            }

            var applicationInfo = new T();

            if ( BackstageServiceFactory.Initialize( () => applicationInfo, applicationInfo.Name ) )
            {
                Logger.Initialize();
            }

            // Set the field at the end to avoid data races.
            _applicationInfo = applicationInfo;
        }
    }
}