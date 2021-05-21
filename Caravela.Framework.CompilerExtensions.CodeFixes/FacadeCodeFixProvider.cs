// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.CompilerExtensions
{
    // ReSharper disable UnusedType.Global

    [ExportCodeFixProvider( LanguageNames.CSharp, Name = nameof(FacadeCodeFixProvider) )]
    [Shared]
    public class FacadeCodeFixProvider : CodeFixProvider
    {
        public override Task RegisterCodeFixesAsync( CodeFixContext context )
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Mark type as partial",
                    cancellationToken => GetFixedDocument( context.Document, context.Span, cancellationToken ),
                    "MakePartial" ),
                context.Diagnostics );

            return Task.CompletedTask;
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        private static async Task<Document> GetFixedDocument( Document document, TextSpan span, CancellationToken cancellationToken )
        {
            var syntaxRoot = await document.GetSyntaxRootAsync( cancellationToken );

            if ( syntaxRoot == null )
            {
                return document;
            }

            var node = syntaxRoot.FindNode( span );
            var typeDeclaration = GetTypeDeclaration( node );

            if ( typeDeclaration == null )
            {
                return document;
            }

            var newTypeDeclaration = typeDeclaration.AddModifiers( SyntaxFactory.Token( SyntaxKind.PartialKeyword ) );
            var newSyntaxRoot = syntaxRoot.ReplaceNode( typeDeclaration, newTypeDeclaration );
            var newDocument = document.WithSyntaxRoot( newSyntaxRoot );

            return newDocument;
        }

        private static BaseTypeDeclarationSyntax? GetTypeDeclaration( SyntaxNode node )
            => node switch
            {
                BaseTypeDeclarationSyntax typeDeclaration => typeDeclaration,
                { Parent: { } parent } => GetTypeDeclaration( parent ),
                _ => null
            };

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create( "CR0307" );
    }
}