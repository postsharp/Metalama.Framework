// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class VsDiagnosticAnalyzer : TheDiagnosticAnalyzer
{
    public VsDiagnosticAnalyzer( ServiceProvider serviceProvider ) : base( serviceProvider ) { }

    public VsDiagnosticAnalyzer() : this( VsServiceProviderFactory.GetServiceProvider() ) { }
}