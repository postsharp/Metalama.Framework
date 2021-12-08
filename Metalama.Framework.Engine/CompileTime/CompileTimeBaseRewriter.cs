// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// A base <see cref="CSharpSyntaxRewriter"/> that stores the <see cref="RunTimeCompilation"/> and the <see cref="SymbolClassifier"/>.
    /// </summary>
    internal abstract partial class CompileTimeBaseRewriter : CSharpSyntaxRewriter
    {
        protected ISymbolClassifier SymbolClassifier { get; }

        protected CompileTimeBaseRewriter( Compilation runTimeCompilation, IServiceProvider serviceProvider )
        {
            this.SymbolClassifier = serviceProvider.GetService<SymbolClassificationService>().GetClassifier( runTimeCompilation );
            this.RunTimeCompilation = runTimeCompilation;
        }

        protected Compilation RunTimeCompilation { get; }

        protected virtual T RewriteThrowNotSupported<T>( T node )
            where T : SyntaxNode
            => node;

        protected MethodDeclarationSyntax WithThrowNotSupportedExceptionBody( MethodDeclarationSyntax method, string message )
        {
            // Method does not have a body (e.g. because it's abstract) , so there is nothing to replace.
            if ( method.Body == null && method.ExpressionBody == null )
            {
                // Should not be called with an abstract method.
                throw new ArgumentOutOfRangeException( nameof(method) );
            }

            return this.RewriteThrowNotSupported(
                method
                    .WithBody( null )
                    .WithExpressionBody( ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                    .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) )
                    .WithModifiers( TokenList( method.Modifiers.Where( m => m.Kind() != SyntaxKind.AsyncKeyword ) ) )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( method.GetLeadingTrivia() )
                    .WithTrailingTrivia( LineFeed, LineFeed ) );
        }

        protected BasePropertyDeclarationSyntax WithThrowNotSupportedExceptionBody( BasePropertyDeclarationSyntax memberDeclaration, string message )
        {
            if ( memberDeclaration.Modifiers.Any( x => x.Kind() == SyntaxKind.AbstractKeyword ) )
            {
                // Should not be called with an abstract property.
                throw new ArgumentOutOfRangeException( nameof(memberDeclaration) );
            }

            switch ( memberDeclaration )
            {
                case PropertyDeclarationSyntax { ExpressionBody: { } } property:
                    // Expression bodied property - change the expression to throw exception.
                    return this.RewriteThrowNotSupported(
                        property
                            .WithExpressionBody( property.ExpressionBody?.WithExpression( GetNotSupportedExceptionExpression( message ) ) )
                            .NormalizeWhitespace()
                            .WithLeadingTrivia( property.GetLeadingTrivia() )
                            .WithTrailingTrivia( LineFeed, LineFeed ) );

                case PropertyDeclarationSyntax { AccessorList: { } } property:
                    // Property with accessor list - change all accessors to expression bodied which do throw exception, remove initializer.

                    return this.RewriteThrowNotSupported(
                        property
                            .WithAccessorList(
                                property.AccessorList!.WithAccessors(
                                    List(
                                        property.AccessorList.Accessors.Select(
                                            x => x
                                                .WithBody( null )
                                                .WithExpressionBody( ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                                                .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) ) ) ) )
                            .WithInitializer( null )
                            .NormalizeWhitespace()
                            .WithLeadingTrivia( property.GetLeadingTrivia() )
                            .WithTrailingTrivia( LineFeed, LineFeed ) );

                case IndexerDeclarationSyntax { AccessorList: { } }:
                    throw new AssertionFailedException( "Build-time indexers are not supported." );
                /*
                // Property with accessor list - change all accessors to expression bodied which do throw exception, remove initializer.

                return indexer
                    .WithAccessorList(
                        indexer.AccessorList!.WithAccessors(
                            List(
                                indexer.AccessorList.Accessors.Select(
                                    x => x
                                        .WithBody( null )
                                        .WithExpressionBody( ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                                        .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) ) ) ) )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( indexer.GetLeadingTrivia() )
                    .WithTrailingTrivia( LineFeed, LineFeed );
                    */

                case EventDeclarationSyntax @event:
                    // Event with accessor list.

                    return this.RewriteThrowNotSupported(
                        @event
                            .WithAccessorList(
                                @event.AccessorList.AssertNotNull()
                                    .WithAccessors(
                                        List(
                                            @event.AccessorList.AssertNotNull()
                                                .Accessors.Select(
                                                    x => x
                                                        .WithBody( null )
                                                        .WithExpressionBody( ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                                                        .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) ) ) ) )
                            .NormalizeWhitespace()
                            .WithLeadingTrivia( @event.GetLeadingTrivia() )
                            .WithTrailingTrivia( LineFeed, LineFeed ) );

                default:
                    throw new AssertionFailedException();
            }
        }

        /*
        protected static IEnumerable<EventDeclarationSyntax> WithThrowNotSupportedExceptionBody( EventFieldDeclarationSyntax @eventField, string message )
        {
            // Event with accessor list.

            foreach ( var declarator in @eventField.Declaration.Variables )
            {
                yield return
                    EventDeclaration(
                        List<AttributeListSyntax>(),
                        eventField.Modifiers,
                        eventField.Declaration.Type,
                        null,
                        declarator.Identifier,
                        AccessorList(
                            List(
                                new[]
                                {
                                    AccessorDeclaration(
                                        SyntaxKind.AddAccessorDeclaration,
                                        List<AttributeListSyntax>(),
                                        TokenList(),
                                        ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) ),
                                    AccessorDeclaration(
                                        SyntaxKind.RemoveAccessorDeclaration,
                                        List<AttributeListSyntax>(),
                                        TokenList(),
                                        ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                                } ) ) );
            }
        }
        */

        private static ThrowExpressionSyntax GetNotSupportedExceptionExpression( string message )
            =>

                // throw new System.NotSupportedException("message")
                ThrowExpression(
                    ObjectCreationExpression( ParseTypeName( "System.NotSupportedException" ) )
                        .AddArgumentListArguments( Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( message ) ) ) ) );
    }
}