﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
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
            // Method does not have a body (e.g. because it's abstract) , so there is nothing to replace
            if ( method.Body == null && method.ExpressionBody == null )
            {
                return method;
            }

            // throw new System.NotSupportedException("message")
            var body = ThrowExpression(
                ObjectCreationExpression( ParseTypeName( "System.NotSupportedException" ) )
                    .AddArgumentListArguments( Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( message ) ) ) ) );

            return method
                .WithBody( null )
                .WithExpressionBody( ArrowExpressionClause( body ) )
                .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) )
                .NormalizeWhitespace()
                .WithLeadingTrivia( method.GetLeadingTrivia() )
                .WithTrailingTrivia( LineFeed, LineFeed );
        }

        protected SymbolDeclarationScope GetSymbolDeclarationScope( MemberDeclarationSyntax node )
        {
            var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;

            return this.SymbolClassifier.GetSymbolDeclarationScope( symbol );
        }
    }
}