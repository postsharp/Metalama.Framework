// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.CodeFixes;
using System.Collections.Immutable;

namespace Metalama.Framework.Diagnostics;

public interface IDiagnostic
{
    IDiagnosticDefinition Definition { get; }

    ImmutableArray<CodeFix> CodeFixes { get; }

    object? Arguments { get; }

    IDiagnostic WithCodeFixes( params CodeFix[] codeFixes );

    void ReportTo( IDiagnosticLocation location, IDiagnosticSink sink );

    void ReportTo( in ScopedDiagnosticSink sink );
}