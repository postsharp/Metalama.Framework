// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Threading;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;

internal sealed class TestCodeRefactoringContext : ICodeRefactoringContext
{
    public TestCodeRefactoringContext( Document document, TextSpan span )
    {
        this.Document = document;
        this.Span = span;
    }

    public Document Document { get; }

    public TextSpan Span { get; }

    public CancellationToken CancellationToken => CancellationToken.None;

    public void RegisterRefactoring( CodeAction action ) => this.RegisteredRefactorings.Add( action );

    public List<CodeAction> RegisteredRefactorings { get; } = new();
}