// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.VisualStudio;

#pragma warning disable  RS1001 // No DiagnosticAnalyzerAttribute

public class VsDiagnosticSuppressor : TheDiagnosticSuppressor
{
    public VsDiagnosticSuppressor( ServiceProvider<IGlobalService> serviceProvider ) : base( serviceProvider ) { }

    public VsDiagnosticSuppressor() : this( VsServiceProviderFactory.GetServiceProvider() ) { }
}