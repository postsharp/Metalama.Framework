// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.DesignTime.Services;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Rider;

[UsedImplicitly]
internal sealed class RiderCodeRefactoringProvider : TheCodeRefactoringProvider
{
    private readonly TheCodeFixProvider _codeFixProvider;
    private readonly TheDiagnosticAnalyzer _diagnosticAnalyzer;

    public RiderCodeRefactoringProvider() : this( DesignTimeServiceProviderFactory.GetSharedServiceProvider() ) { }

    public RiderCodeRefactoringProvider( GlobalServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._codeFixProvider = new TheCodeFixProvider( serviceProvider );
        this._diagnosticAnalyzer = new TheDiagnosticAnalyzer( serviceProvider );
    }

    internal override async Task ComputeRefactoringsAsync( ICodeRefactoringContext context )
    {
        // Report regular refactorings.
        await base.ComputeRefactoringsAsync( context );

        // Compute hidden diagnostics for the given span.
        var semanticModel = await context.Document.GetSemanticModelAsync( context.CancellationToken );

        if ( semanticModel == null )
        {
            return;
        }

        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        this._diagnosticAnalyzer.AnalyzeSemanticModel(
            new SemanticModelAnalysisContext(
                semanticModel,
                context.Document.Project.AnalyzerOptions,
                diagnostic =>
                {
                    if ( diagnostic.Severity == DiagnosticSeverity.Hidden && diagnostic.Location.SourceSpan.IntersectsWith( context.Span ) )
                    {
                        diagnostics.Add( diagnostic );
                    }
                },
                _ => true,
                context.CancellationToken ) );

        // Report code fixes for found diagnostics as refactorings.
        await this._codeFixProvider.RegisterCodeFixesAsync( new HiddenCodeFixToCodeRefactoringContext( context, diagnostics.ToImmutable() ) );
    }

    private sealed class HiddenCodeFixToCodeRefactoringContext : ICodeFixContext
    {
        private readonly ICodeRefactoringContext _codeRefactoringContext;

        public HiddenCodeFixToCodeRefactoringContext( ICodeRefactoringContext codeRefactoringContext, ImmutableArray<Diagnostic> diagnostics )
        {
            this._codeRefactoringContext = codeRefactoringContext;
            this.Diagnostics = diagnostics;
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public Document Document => this._codeRefactoringContext.Document;

        public TextSpan Span => this._codeRefactoringContext.Span;

        public CancellationToken CancellationToken => this._codeRefactoringContext.CancellationToken;

        public void RegisterCodeFix( CodeAction action, ImmutableArray<Diagnostic> diagnostics )
            => this._codeRefactoringContext.RegisterRefactoring( action );
    }
}