// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Analyzers;

[ExportCodeFixProvider( LanguageNames.CSharp )]
[UsedImplicitly]
public class MetalamaPerformanceCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create( MetalamaPerformanceAnalyzer._normalizeWhitespace.Id, MetalamaPerformanceAnalyzer._syntaxNodeWith.Id );

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync( CodeFixContext context )
    {
        foreach ( var diagnostic in context.Diagnostics )
        {
            if ( diagnostic.Id == MetalamaPerformanceAnalyzer._normalizeWhitespace.Id )
            {
                await RegisterNormalizeWhitespaceCodeFixAsync( context, diagnostic );
            }
            else if ( diagnostic.Id == MetalamaPerformanceAnalyzer._syntaxNodeWith.Id )
            {
                await RegisterSyntaxNodeWithCodeFixAsync( context, diagnostic );
            }
        }
    }

    private static async Task RegisterNormalizeWhitespaceCodeFixAsync( CodeFixContext context, Diagnostic diagnostic )
    {
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync( context.CancellationToken );
        var node = root.FindNode( diagnostic.Location.SourceSpan );

        if ( node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: var expression } } )
        {
            const string Title = "Remove call to NormalizeWhitespace";

            context.RegisterCodeFix( CodeAction.Create( Title, ct =>
            {
                var newRoot = root.ReplaceNode( node, expression );

                return Task.FromResult( document.WithSyntaxRoot( newRoot ) );
            }, equivalenceKey: Title ), diagnostic );
        }
    }

    private static async Task RegisterSyntaxNodeWithCodeFixAsync( CodeFixContext context, Diagnostic diagnostic )
    {
        //var document = context.Document;
        //var root = await document.GetSyntaxRootAsync( context.CancellationToken );
        //var node = root.FindNode( diagnostic.Location.SourceSpan );

        //var semanticModel = await document.GetSemanticModelAsync();

        //IEnumerable<object> GetSyntaxNodeWithInvocations( SyntaxNode node )
        //{
        //    var operation = semanticModel.GetOperation( node );

        //    if (node is )
        //}

        //var syntaxNodeWithInvocations = 
    }
}