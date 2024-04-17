// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Rpc.Notifications;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Services;

internal class DesignTimeInvalidationService : IGlobalService
{
    private readonly WorkspaceProvider _workspaceProvider;

    public DesignTimeInvalidationService( GlobalServiceProvider serviceProvider )
    {
        this._workspaceProvider = serviceProvider.GetRequiredService<WorkspaceProvider>();

        var hub = serviceProvider.GetRequiredService<AnalysisProcessEventHub>();
        hub.CompilationResultChanged += this.OnCompilationResultChanged;
    }

    private void OnCompilationResultChanged( CompilationResultChangedEventArgs args )
    {
        if ( !this._workspaceProvider.TryGetWorkspace( out var workspace ) )
        {
            // The workspace is not available yet.
            return;
        }

        GetDiagnosticsRefresherInvoker( workspace )?.Invoke();
    }

    private static readonly WeakCache<Workspace, Action?> _diagnosticsRefresherInvokers = new();

    private static Action? GetDiagnosticsRefresherInvoker( Workspace workspace )
    {
        return _diagnosticsRefresherInvokers.GetOrAdd( workspace, GetDiagnosticsRefresherInvokerCore );

        static Action? GetDiagnosticsRefresherInvokerCore( Workspace workspace )
        {
            // This is equivalent to the following code that uses internal Roslyn types:
            // ((IMefHostExportProvider)workspace.Services.HostServices).GetExports<IDiagnosticsRefresher>().SingleOrDefault()?.Value.RequestWorkspaceRefresh();

            // In this whole code, nulls should only happen if Roslyn internals change.
            // The one exception is lazyExport, which is null if there are no exports of IDiagnosticsRefresher, which happens when LanguageServer is not used (i.e. in VS and Rider).

            var codeAnalysisFeaturesAssembly = typeof( Microsoft.CodeAnalysis.Completion.CompletionProvider ).Assembly;
            var iDiagnosticsRefresher = codeAnalysisFeaturesAssembly.GetType( "Microsoft.CodeAnalysis.Diagnostics.IDiagnosticsRefresher" );
            var requestWorkspaceRefresh = iDiagnosticsRefresher?.GetMethod( "RequestWorkspaceRefresh", Type.EmptyTypes );

            if ( iDiagnosticsRefresher == null || requestWorkspaceRefresh == null )
            {
                return null;
            }

            var codeAnalysisWorkspacesAssembly = typeof( Workspace ).Assembly;
            var iMefHostExportProvider = codeAnalysisWorkspacesAssembly.GetType( "Microsoft.CodeAnalysis.Host.Mef.IMefHostExportProvider" );
            var getExports = iMefHostExportProvider?.GetMethods().SingleOrDefault( m => m.Name == "GetExports" && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 0 );
            var getExportsOfDiagnosticRefresher = getExports?.MakeGenericMethod( iDiagnosticsRefresher );

            var hostServices = workspace.Services.HostServices;

            var exports = getExportsOfDiagnosticRefresher?.Invoke( hostServices, [] ) as IEnumerable<object>;
            var lazyExport = exports?.SingleOrDefault();
            var export = lazyExport?.GetType().GetProperty( "Value" )?.GetValue( lazyExport );

            if ( export == null )
            {
                return null;
            }

            return () => requestWorkspaceRefresh.Invoke( export, [] );
        }
    }
}