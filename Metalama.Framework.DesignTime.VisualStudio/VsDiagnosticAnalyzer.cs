// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.DesignTime.VisualStudio;

#pragma warning disable  RS1001 // No DiagnosticAnalyzerAttribute

public class VsDiagnosticAnalyzer : TheDiagnosticAnalyzer
{
    public VsDiagnosticAnalyzer( ServiceProvider serviceProvider ) : base( serviceProvider ) { }

    public VsDiagnosticAnalyzer() : this( VsServiceProviderFactory.GetServiceProvider() ) { }
}