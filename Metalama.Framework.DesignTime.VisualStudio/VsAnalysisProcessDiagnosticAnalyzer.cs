// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.VisualStudio.Services;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Metalama.Framework.DesignTime.VisualStudio;

#pragma warning disable RS1001, RS1022, RS1025, RS1026

[UsedImplicitly]
public class VsAnalysisProcessDiagnosticAnalyzer : TheDiagnosticAnalyzer
{
    public VsAnalysisProcessDiagnosticAnalyzer( ServiceProvider<IGlobalService> serviceProvider ) : base( serviceProvider ) { }

    public VsAnalysisProcessDiagnosticAnalyzer() : this( VsServiceProviderFactory.GetServiceProvider() ) { }

    private HashSet<ProjectKey>? _registeredProjects;

    public override void Initialize( AnalysisContext context )
    {
        base.Initialize( context );

        // It seems that in packages.config projects, the source generator is not run reliably in the RoslynCodeAnalysisService process.
        // This is a problem, because the devenv generator normally depends on the RoslynCodeAnalysisService generator being run before.
        // To fix that, create VsAnalysisProcessProjectHandler, which registers the project on the endpoint, the same as if the source generator was run.
        context.RegisterCompilationAction(
            compilationContext =>
            {
                var options = MSBuildProjectOptionsFactory.Default.GetProjectOptions( compilationContext.Options.AnalyzerConfigOptionsProvider );

                if ( options is { IsFrameworkEnabled: true, IsDesignTimeEnabled: true, UsesPackagesConfig: true } )
                {
                    var projectKey = compilationContext.Compilation.GetProjectKey();

                    if ( !projectKey.HasHashCode )
                    {
                        return;
                    }

                    this._registeredProjects ??= new();

                    if ( this._registeredProjects.Add( projectKey ) )
                    {
                        _ = new VsAnalysisProcessProjectHandler( VsServiceProviderFactory.GetServiceProvider(), options, projectKey );
                    }
                }
            } );
    }
}