// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.Introspection;

public class CompilationFailedException : Exception
{
    public CompilationFailedException( string message, ImmutableArray<IIntrospectionDiagnostic> diagnostics ) : base( message )
    {
        this.Diagnostics = diagnostics;
    }

    public ImmutableArray<IIntrospectionDiagnostic> Diagnostics { get; }
}