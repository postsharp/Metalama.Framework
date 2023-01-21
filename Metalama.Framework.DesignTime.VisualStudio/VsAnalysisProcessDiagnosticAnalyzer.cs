// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.DesignTime.VisualStudio.Services;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.VisualStudio;

#pragma warning disable RS1001, RS1022

[UsedImplicitly]
public class VsAnalysisProcessDiagnosticAnalyzer : TheDiagnosticAnalyzer
{
    public VsAnalysisProcessDiagnosticAnalyzer( ServiceProvider<IGlobalService> serviceProvider ) : base( serviceProvider ) { }

    public VsAnalysisProcessDiagnosticAnalyzer() : this( VsServiceProviderFactory.GetServiceProvider() ) { }
}