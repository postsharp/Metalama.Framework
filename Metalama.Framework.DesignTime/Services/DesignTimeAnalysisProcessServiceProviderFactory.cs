// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using Metalama.Framework.DesignTime.AspectExplorer;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.Services;

public class DesignTimeAnalysisProcessServiceProviderFactory : DesignTimeServiceProviderFactory
{
    public DesignTimeAnalysisProcessServiceProviderFactory() : this( null ) { }

    public DesignTimeAnalysisProcessServiceProviderFactory( DesignTimeEntryPointManager? entryPointManager ) : base( entryPointManager ) { }

    private static WorkspaceProvider GetWorkspaceProvider( GlobalServiceProvider serviceProvider )
    {
        // TODO: WorkspaceProvider should be refactored as an IGlobalService.
        
        switch ( ProcessUtilities.ProcessKind )
        {
            case ProcessKind.Rider:
            case ProcessKind.OmniSharp:
            case ProcessKind.VisualStudioMac:
            case ProcessKind.LanguageServer:
                return new LocalWorkspaceProvider( serviceProvider );

            default:
                if ( RemoteWorkspaceProvider.TryCreate( serviceProvider, out var workspaceProvider ) )
                {
                    return workspaceProvider;
                }
                else
                {
                    // This is used in tests, when we test the DesignTimeServiceProviderFactory class.
                    return new FakeWorkspaceProvider( serviceProvider );
                }
        }
    }

    protected override ServiceProvider<IGlobalService> AddServices( ServiceProvider<IGlobalService> serviceProvider )
    {
        // Initialize the event hub.
        serviceProvider = base.AddServices( serviceProvider )
            .WithServices( new AnalysisProcessEventHub( serviceProvider ) );

        serviceProvider = serviceProvider.WithService( GetWorkspaceProvider( serviceProvider ) );

        // Add the pipeline factory.
        var pipelineFactory = new DesignTimeAspectPipelineFactory( serviceProvider, new CompileTimeDomain( serviceProvider, "DesignTime" ) );
        serviceProvider = serviceProvider.WithServices( pipelineFactory );

        // Add services that depend on the pipeline factory.
        serviceProvider = serviceProvider.WithServices(
            new CodeActionExecutionService( serviceProvider ),
            new CodeRefactoringDiscoveryService( serviceProvider ),
            new AspectDatabase( serviceProvider ) );

        return serviceProvider;
    }
}