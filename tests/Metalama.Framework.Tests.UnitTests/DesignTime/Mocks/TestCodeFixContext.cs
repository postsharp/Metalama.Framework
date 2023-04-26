// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;

internal sealed class TestCodeFixContext : ICodeFixContext
{
    public TestCodeFixContext( Document document, ImmutableArray<Diagnostic> diagnostics, TextSpan span )
    {
        this.Document = document;
        this.Diagnostics = diagnostics;
        this.Span = span;
    }

    public Document Document { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public TextSpan Span { get; }

    public CancellationToken CancellationToken => CancellationToken.None;

    public void RegisterCodeFix( CodeAction action, ImmutableArray<Diagnostic> diagnostics )
    {
        this.RegisteredCodeFixes.Add( (action, diagnostics) );
    }

    public List<(CodeAction CodeAction, ImmutableArray<Diagnostic> Diagnostics)> RegisteredCodeFixes { get; } = new();
}