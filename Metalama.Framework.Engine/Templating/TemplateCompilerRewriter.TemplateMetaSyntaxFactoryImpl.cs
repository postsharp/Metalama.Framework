// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CompileTimeContracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#pragma warning disable CA1822

namespace Metalama.Framework.Engine.Templating
{
    internal sealed partial class TemplateCompilerRewriter
    {
        public const string TemplateSyntaxFactoryParameterName = "templateSyntaxFactory";

        /// <summary>
        /// Generates code that invokes members of the <see cref="ITemplateSyntaxFactory"/> class.
        /// </summary>
        private class TemplateMetaSyntaxFactoryImpl
        {
            /// <summary>
            /// Generates a <see cref="MemberAccessExpressionSyntax"/> that represents the fully-qualified name
            /// of a method of the <see cref="ITemplateSyntaxFactory"/> class.
            /// </summary>
            /// <param name="name">Name of the method.</param>
            /// <returns></returns>
            public MemberAccessExpressionSyntax TemplateSyntaxFactoryMember( string name )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName( TemplateSyntaxFactoryParameterName ),
                    SyntaxFactory.IdentifierName( name ) );

            public MemberAccessExpressionSyntax GenericTemplateSyntaxFactoryMember( string name, params TypeSyntax[] genericParameters )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName( TemplateSyntaxFactoryParameterName ),
                    SyntaxFactory.GenericName( SyntaxFactory.Identifier( name ) )
                        .WithTypeArgumentList( SyntaxFactory.TypeArgumentList( SyntaxFactory.SeparatedList( genericParameters ) ) ) );

            /// <summary>
            /// Generates a call to the <see cref="ITemplateSyntaxFactory.GetUniqueIdentifier"/> method.
            /// </summary>
            /// <param name="hint"></param>
            /// <returns></returns>
            public ExpressionSyntax GetUniqueIdentifier( string hint )
                => SyntaxFactory.InvocationExpression( this.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.GetUniqueIdentifier) ) )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[] { SyntaxFactory.Argument( SyntaxFactoryEx.LiteralExpression( hint ) ) } ) ) );
        }
    }
}