// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// A base <see cref="CSharpSyntaxRewriter"/> that stores the <see cref="RunTimeCompilation"/> and the <see cref="SymbolClassifier"/>.
    /// </summary>
    internal abstract class CompileTimeBaseRewriter : CSharpSyntaxRewriter
    {
        public ISymbolClassifier SymbolClassifier { get; }

        protected CompileTimeBaseRewriter( Compilation runTimeCompilation, IServiceProvider serviceProvider )
        {
            this.SymbolClassifier = serviceProvider.GetService<SymbolClassificationService>().GetClassifier( runTimeCompilation );
            this.RunTimeCompilation = runTimeCompilation;
        }

        protected Compilation RunTimeCompilation { get; }

        protected static MethodDeclarationSyntax WithThrowNotSupportedExceptionBody( MethodDeclarationSyntax method, string message )
        {
            // Method does not have a body (e.g. because it's abstract) , so there is nothing to replace.
            if ( method.Body == null && method.ExpressionBody == null )
            {
                return method;
            }

            return method
                .WithBody( null )
                .WithExpressionBody( ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) )
                .NormalizeWhitespace()
                .WithLeadingTrivia( method.GetLeadingTrivia() )
                .WithTrailingTrivia( LineFeed, LineFeed );
        }

        protected static BasePropertyDeclarationSyntax WithThrowNotSupportedExceptionBody( BasePropertyDeclarationSyntax baseProperty, string message )
        {
            if ( baseProperty.Modifiers.Any( x => x.Kind() == SyntaxKind.AbstractKeyword ) )
            {
                // Abstract property - we don't have to do anything with it.
                return baseProperty;
            }

            if ( baseProperty is PropertyDeclarationSyntax property )
            {
                if ( baseProperty is PropertyDeclarationSyntax && property.ExpressionBody != null )
                {
                    // Expression bodied property - change the expression to throw exception.
                    return property
                        .WithExpressionBody( property.ExpressionBody?.WithExpression( GetNotSupportedExceptionExpression( message ) ) )
                        .NormalizeWhitespace()
                        .WithLeadingTrivia( property.GetLeadingTrivia() )
                        .WithTrailingTrivia( LineFeed, LineFeed );
                }

                if ( property.AccessorList != null )
                {
                    // Property with accessor list - change all accessors to expression bodied which do throw exception, remove initializer.

                    return property
                        .WithAccessorList(
                            property.AccessorList.WithAccessors(
                                List(
                                    property.AccessorList.Accessors.Select(
                                        x => x
                                            .WithBody( null )
                                            .WithExpressionBody( ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                                            .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) ) ) ) )
                        .WithInitializer( null )
                        .NormalizeWhitespace()
                        .WithLeadingTrivia( property.GetLeadingTrivia() )
                        .WithTrailingTrivia( LineFeed, LineFeed );
                }
            }
            else if ( baseProperty is IndexerDeclarationSyntax indexer )
            {
                if ( indexer.AccessorList != null )
                {
                    // Property with accessor list - change all accessors to expression bodied which do throw exception, remove initializer.

                    return indexer
                        .WithAccessorList(
                            indexer.AccessorList.WithAccessors(
                                List(
                                    indexer.AccessorList.Accessors.Select(
                                        x => x
                                            .WithBody( null )
                                            .WithExpressionBody( ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                                            .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) ) ) ) )
                        .NormalizeWhitespace()
                        .WithLeadingTrivia( indexer.GetLeadingTrivia() )
                        .WithTrailingTrivia( LineFeed, LineFeed );
                }
            }

            throw new AssertionFailedException();
        }

        private static ThrowExpressionSyntax GetNotSupportedExceptionExpression( string message )
        {
            // throw new System.NotSupportedException("message")
            return ThrowExpression(
                ObjectCreationExpression( ParseTypeName( "System.NotSupportedException" ) )
                    .AddArgumentListArguments( Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( message ) ) ) ) );
        }

        protected TemplatingScope GetTemplatingScope( MemberDeclarationSyntax node )
        {
            var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;

            return this.SymbolClassifier.GetTemplatingScope( symbol );
        }
    }
}