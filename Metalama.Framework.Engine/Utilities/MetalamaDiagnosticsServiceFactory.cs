// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;

namespace Metalama.Framework.Engine.Utilities
{
    public static class MetalamaDiagnosticsServiceFactory
    {
        public static void Initialize(string caller, string? projectName = null)
        {
            if ( DiagnosticServiceFactory.Initialize( () => new MetalamaApplicationInfo(), caller, projectName ) )
            {
                Logger.Initialize();
            }
        }
    }
}