// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Templating
{
    internal sealed partial class TemplateCompilerRewriter
    {
        private class CompileTimeOnlyRewriter : CSharpSyntaxRewriter
        {
            private readonly TemplateCompilerRewriter _parent;

            public CompileTimeOnlyRewriter( TemplateCompilerRewriter parent )
            {
                this._parent = parent;
            }

            public override SyntaxNode? VisitTypeOfExpression( TypeOfExpressionSyntax node )
            {
                if ( this._parent._syntaxTreeAnnotationMap.GetSymbol( node.Type ) is ITypeSymbol typeSymbol )
                {
                    var typeId = SymbolId.Create( typeSymbol ).Id;

                    return this._parent._typeOfRewriter.RewriteTypeOf( typeSymbol, this._parent.CreateTypeParameterSubstitutionDictionary( nameof( TemplateTypeArgument.Syntax ) ) )
                        .WithAdditionalAnnotations( new SyntaxAnnotation( _rewrittenTypeOfAnnotation, typeId ) );
                }
                else
                {
                    return node;
                }
            }

            public bool TryRewriteProceedInvocation( InvocationExpressionSyntax node, out InvocationExpressionSyntax transformedNode )
            {
                var kind = this._parent._templateMemberClassifier.GetMetaMemberKind( node.Expression );

                if ( kind.IsAnyProceed() )
                {
                    var methodName = node.Expression switch
                    {
                        MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                        IdentifierNameSyntax identifier => identifier.Identifier.Text,
                        _ => throw new AssertionFailedException( $"Don't know how to get the member name in {node.Expression.GetType().Name}" )
                    };

                    // ReSharper disable once RedundantSuppressNullableWarningExpression
                    transformedNode =
                        node.CopyAnnotationsTo(
                            InvocationExpression( this._parent._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.Proceed) ) )
                                .WithArgumentList( ArgumentList( SeparatedList( new[] { Argument( SyntaxFactoryEx.LiteralExpression( methodName ) ) } ) ) ) )!;

                    return true;
                }
                else
                {
                    transformedNode = node;

                    return false;
                }
            }

            public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
            {
                if ( this.TryRewriteProceedInvocation( node, out var transformedNode ) )
                {
                    return transformedNode;
                }
                else if ( node.IsNameOf() )
                {
                    var type = this._parent._syntaxTreeAnnotationMap.GetSymbol( node.ArgumentList.Arguments[0].Expression );

                    if ( type != null )
                    {
                        return SyntaxFactoryEx.LiteralExpression( type.Name );
                    }
                }

                return base.VisitInvocationExpression( node );
            }
        }
    }
}