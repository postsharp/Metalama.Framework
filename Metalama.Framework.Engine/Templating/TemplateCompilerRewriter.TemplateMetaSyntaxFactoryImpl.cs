// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Impl.Templating
{
    internal sealed partial class TemplateCompilerRewriter
    {
        /// <summary>
        /// Generates code that invokes members of the <see cref="TemplateSyntaxFactory"/> class.
        /// </summary>
        private class TemplateMetaSyntaxFactoryImpl
        {
            private readonly MetaSyntaxFactoryImpl _metaSyntaxFactory;

            public TemplateMetaSyntaxFactoryImpl( MetaSyntaxFactoryImpl metaSyntaxFactory )
            {
                this._metaSyntaxFactory = metaSyntaxFactory;
            }

            /// <summary>
            /// Generates a <see cref="MemberAccessExpressionSyntax"/> that represents the fully-qualified name
            /// of a method of the <see cref="TemplateSyntaxFactory"/> class.
            /// </summary>
            /// <param name="name">Name of the method.</param>
            /// <returns></returns>
            public MemberAccessExpressionSyntax TemplateSyntaxFactoryMember( string name )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this._metaSyntaxFactory.Type( typeof(TemplateSyntaxFactory) ),
                    SyntaxFactory.IdentifierName( name ) );

            public MemberAccessExpressionSyntax GenericTemplateSyntaxFactoryMember( string name, params TypeSyntax[] genericParameters )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this._metaSyntaxFactory.Type( typeof(TemplateSyntaxFactory) ),
                    SyntaxFactory.GenericName( SyntaxFactory.Identifier( name ) )
                        .WithTypeArgumentList( SyntaxFactory.TypeArgumentList( SyntaxFactory.SeparatedList( genericParameters ) ) ) );

            /// <summary>
            /// Generates a call to the <see cref="TemplateSyntaxFactory.GetUniqueIdentifier"/> method.
            /// </summary>
            /// <param name="hint"></param>
            /// <returns></returns>
            public ExpressionSyntax GetUniqueIdentifier( string hint )
                => SyntaxFactory.InvocationExpression( this.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.GetUniqueIdentifier) ) )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[] { SyntaxFactory.Argument( SyntaxFactoryEx.LiteralExpression( hint ) ) } ) ) );
        }
    }
}