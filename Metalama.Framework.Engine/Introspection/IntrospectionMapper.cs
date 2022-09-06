// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Introspection;

public static class IntrospectionMapper
{
    public static ImmutableArray<IIntrospectionDiagnostic> ToReportedDiagnostics(
        this ImmutableArray<Diagnostic> diagnostics,
        ICompilation compilation,
        DiagnosticSource source )
        => diagnostics.Select( x => new IntrospectionDiagnostic( x, compilation, source ) ).ToImmutableArray<IIntrospectionDiagnostic>();

    public static IIntrospectionAspectClass AggregateAspectClasses( IAspectClass aspectClass, IEnumerable<IIntrospectionAspectInstance> instances )
        => new AggregatedIntrospectionAspectClass( aspectClass, instances );
}