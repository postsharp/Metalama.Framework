// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.DesignTime.Services;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Rider;

[UsedImplicitly]
public sealed class RiderCodeFixProvider : TheCodeFixProvider
{
    public RiderCodeFixProvider() : this( DesignTimeServiceProviderFactory.GetSharedServiceProvider() ) { }

    public RiderCodeFixProvider( GlobalServiceProvider serviceProvider ) : base( serviceProvider ) { }

    protected override bool SkipDiagnostic( Diagnostic diagnostic ) => diagnostic.Severity == DiagnosticSeverity.Hidden;
}