// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Framework.Engine.Utilities.Diagnostics
{
    public static class BackstageServiceFactoryInitializer
    {
        private static BackstageInitializationOptions? _options;

        public static bool IsInitialized => _options != null;

        public static void Initialize( BackstageInitializationOptions options )
        {
            if ( _options != null )
            {
                if ( _options.ApplicationInfo.GetType() != options.ApplicationInfo.GetType() )
                {
                    throw new InvalidOperationException( "The services were initialized with a different implementation of IApplicationInfo." );
                }
                else
                {
                    return;
                }
            }

            if ( BackstageServiceFactory.Initialize( options, options.ApplicationInfo.Name ) )
            {
                Logger.Initialize();
            }

            // Set the field at the end to avoid data races.
            _options = options;
        }
    }
}