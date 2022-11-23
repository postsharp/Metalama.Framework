// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.VisualStudio;

#pragma warning disable  RS1001 // No DiagnosticAnalyzerAttribute

public class VsAnalysisProcessDiagnosticAnalyzer : TheDiagnosticAnalyzer
{
    public VsAnalysisProcessDiagnosticAnalyzer( ServiceProvider<IService> serviceProvider ) : base( serviceProvider ) { }

    public VsAnalysisProcessDiagnosticAnalyzer() : this( VsServiceProviderFactory.GetServiceProvider() ) { }
}