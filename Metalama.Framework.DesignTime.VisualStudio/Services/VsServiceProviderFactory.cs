// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using Metalama.Framework.DesignTime.Services;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.VisualStudio.Services;

/// <summary>
/// Factory of <see cref="GlobalServiceProvider"/> for both user and analysis Visual Studio processes. 
/// </summary>
internal static class VsServiceProviderFactory
{
    public static ServiceProvider<IGlobalService> GetServiceProvider()
    {
        var processKind = ProcessUtilities.ProcessKind;

        switch ( processKind )
        {
            case ProcessKind.DevEnv:
                return DesignTimeServiceProviderFactory.GetSharedServiceProvider<VsUserProcessServiceProviderFactory>();

            case ProcessKind.RoslynCodeAnalysisService:
                return DesignTimeServiceProviderFactory.GetSharedServiceProvider<VsAnalysisProcessServiceProviderFactory>();

            default:
                throw new AssertionFailedException( $"Unexpected process kind: {processKind}." );
        }
    }
}