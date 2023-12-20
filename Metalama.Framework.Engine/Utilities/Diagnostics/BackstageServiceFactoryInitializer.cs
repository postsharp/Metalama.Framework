// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Tools;

namespace Metalama.Framework.Engine.Utilities.Diagnostics
{
    public static class BackstageServiceFactoryInitializer
    {
        [PublicAPI]
        public static bool IsInitialized => BackstageServiceFactory.IsInitialized;

        public static void Initialize( BackstageInitializationOptions options )
        {
            if ( BackstageServiceFactory.Initialize(
                    options with { AddToolsExtractor = builder => builder.AddTools() },
                    options.ApplicationInfo.Name ) )
            {
                Logger.Initialize();
            }
        }
    }
}