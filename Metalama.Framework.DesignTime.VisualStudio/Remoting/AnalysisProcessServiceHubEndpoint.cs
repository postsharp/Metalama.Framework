// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

internal class AnalysisProcessServiceHubEndpoint : ClientEndpoint<IServiceHubApi>, IServiceHubApiProvider
{
    public AnalysisProcessServiceHubEndpoint( IServiceProvider serviceProvider, string pipeName ) : base( serviceProvider, pipeName ) { }

    public static bool TryStart(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken,
        [NotNullWhen( true )] out IServiceHubApiProvider? endpointRegistrationApiProvider )
    {
        if ( !TryGetPipeName( out var pipeName ) )
        {
            endpointRegistrationApiProvider = null;

            return false;
        }

        var endpoint = new AnalysisProcessServiceHubEndpoint( serviceProvider, pipeName );
        _ = endpoint.ConnectAsync( cancellationToken );

        endpointRegistrationApiProvider = endpoint;

        return true;
    }

    private static bool TryGetPipeName( [NotNullWhen( true )] out string? pipeName )
    {
        var parentProcesses = ProcessUtilities.GetParentProcesses();

        Engine.Utilities.Diagnostics.Logger.Remoting.Trace?.Log( $"Parent processes: {string.Join( ", ", parentProcesses.Select( x => x.ToString() ) )}" );

        if ( parentProcesses.Count < 3 ||
             !string.Equals( parentProcesses[1].ProcessName, "Microsoft.ServiceHub.Controller", StringComparison.OrdinalIgnoreCase ) ||
             !string.Equals( parentProcesses[2].ProcessName, "devenv", StringComparison.OrdinalIgnoreCase )
           )
        {
            Engine.Utilities.Diagnostics.Logger.Remoting.Error?.Log( "The process 'devenv' could not be found. " );
            pipeName = null;

            return false;
        }

        var parentProcess = parentProcesses[2];

        pipeName = GetPipeName( ServiceRole.Discovery, parentProcess.ProcessId );

        return true;
    }

    public ValueTask<IServiceHubApi> GetApiAsync( string callerName, CancellationToken cancellationToken )
        => this.GetServerApiAsync( callerName, cancellationToken );
}