// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Rpc.Notifications;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;
using Metalama.Framework.Project;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.AnalysisProcess;

internal class AnalysisProcessServiceHubEndpoint : ClientEndpoint<IServiceHubApi>, IServiceHubApiProvider
{
    private readonly AnalysisProcessEventHub _eventHub;

    public AnalysisProcessServiceHubEndpoint( IServiceProvider serviceProvider, string pipeName ) : base( serviceProvider, pipeName )
    {
        this._eventHub = serviceProvider.GetRequiredService<AnalysisProcessEventHub>();
        this._eventHub.CompilationResultChanged += this.OnCompilationResultChanged;
    }

    private async void OnCompilationResultChanged( CompilationResultChangedEventArgs args )
    {
        this.Logger.Trace?.Log( $"Publishing change notification for project '{args.ProjectKey}'." );

        try
        {
            var api = await this.GetServerApiAsync( nameof(this.OnCompilationResultChanged) );
            await api.NotifyCompilationResultChangedAsync( args, CancellationToken.None );
        }
        catch ( Exception e )
        {
            DesignTimeExceptionHandler.ReportException( e );
        }
    }

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

        Engine.Utilities.Diagnostics.Logger.Remoting.Trace?.Log( $"Parent processes: {string.Join( ", ", parentProcesses.SelectArray( x => x.ToString() ) )}" );

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

        pipeName = PipeNameProvider.GetPipeName( ServiceRole.Discovery, parentProcess.ProcessId );

        return true;
    }

    ValueTask<IServiceHubApi> IServiceHubApiProvider.GetApiAsync( string callerName, CancellationToken cancellationToken )
        => this.GetServerApiAsync( callerName, cancellationToken );

    public void PublishCompilationResultChangedNotification( CompilationResultChangedEventArgs notification )
    {
        this.Logger.Trace?.Log( $"Publishing change notification for '{notification.ProjectKey}.'" );

        Task.Run(
            async () =>
            {
                try
                {
                    var api = await this.GetServerApiAsync( nameof(this.PublishCompilationResultChangedNotification) );
                    await api.NotifyCompilationResultChangedAsync( notification, CancellationToken.None );
                }
                catch ( Exception e )
                {
                    DesignTimeExceptionHandler.ReportException( e, this.Logger );
                }
            } );
    }

    protected override void Dispose( bool disposing )
    {
        this._eventHub.CompilationResultChanged -= this.OnCompilationResultChanged;
    }
}