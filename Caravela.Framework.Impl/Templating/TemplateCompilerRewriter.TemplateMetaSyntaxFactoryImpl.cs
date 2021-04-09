// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal sealed partial class TemplateCompilerRewriter
    {
        private class TemplateMetaSyntaxFactoryImpl
        {
            private MetaSyntaxFactoryImpl _metaSyntaxFactory;

            public TemplateMetaSyntaxFactoryImpl( MetaSyntaxFactoryImpl metaSyntaxFactory )
            {
                this._metaSyntaxFactory = metaSyntaxFactory;
            }

            public MemberAccessExpressionSyntax TemplateSyntaxFactoryMember( string name )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this._metaSyntaxFactory.Type( typeof( TemplateSyntaxFactory ) ),
                    SyntaxFactory.IdentifierName( name ) );
            
            public ExpressionSyntax GetUniqueIdentifier(string hint)
                => SyntaxFactory.InvocationExpression( this.TemplateSyntaxFactoryMember( nameof( TemplateSyntaxFactory.GetUniqueIdentifier ) ) )
                    .WithArgumentList( SyntaxFactory.ArgumentList( SyntaxFactory.SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[]
                    {
                        SyntaxFactory.Argument( this._metaSyntaxFactory.LiteralExpression( hint ) )
                    } ) ) );
        }
    }
}