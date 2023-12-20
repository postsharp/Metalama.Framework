// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Tools;

namespace Metalama.Framework.Engine.Utilities.Diagnostics
{
    public static class BackstageServiceFactoryInitializer
    {
        private static BackstageInitializationOptions? _options;

        [PublicAPI]
        public static bool IsInitialized => _options != null;

        public static void Initialize( BackstageInitializationOptions options )
        {
            if ( _options != null )
            {
                return;
            }

            if ( BackstageServiceFactory.Initialize(
                    options with
                    {
                        AddSupportServices = true, 
                        AddToolsExtractor = builder => builder.AddTools()
                    },
                    options.ApplicationInfo.Name ) )
            {
                Logger.Initialize();
            }

            // Set the field at the end to avoid data races.
            _options = options;
        }
    }
}