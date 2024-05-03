// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Metalama.Framework.Engine.Formatting;

public sealed partial class CodeFormatter
{
    private sealed class CustomSimplifier : SafeSyntaxRewriter
    {
        private readonly SemanticModel? _semanticModel;

        public CustomSimplifier( SemanticModel? semanticModel )
        {
            // The semantic model is optional, but we cannot do all much useful work if we don't have it.
            this._semanticModel = semanticModel;
        }

        public bool RequiresSemanticModel { get; private set; }

        public override SyntaxNode? DefaultVisit( SyntaxNode node )
        {
            if ( this.RequiresSemanticModel )
            {
                // Return as soon as possible.
                return node;
            }
            else
            {
                return base.DefaultVisit( node );
            }
        }

        public override SyntaxNode VisitObjectCreationExpression( ObjectCreationExpressionSyntax node )
        {
            // We want to simplify delegate creation from `new Action( () => { ... } )` to just `() => { ... }`.
            // This simplification is valid when the delegate is target typed. It is implemented for invocation arguments
            // and assignments.
            if ( node.HasAnnotation( Simplifier.Annotation ) )
            {
                if ( node.ArgumentList?.Arguments is
                         [{ Expression: AnonymousFunctionExpressionSyntax anonymousFunctionExpression }]
                     && node.Parent?.Kind() is SyntaxKind.Argument or SyntaxKind.EqualsValueClause )
                {
                    if ( node.Parent is ArgumentSyntax argument )
                    {
                        switch ( node.Parent.Parent?.Parent )
                        {
                            case InvocationExpressionSyntax invocation:
                                if ( this._semanticModel != null )
                                {
                                    var symbol = this._semanticModel.GetSymbolInfo( invocation.Expression ).Symbol;
                                    var argumentIndex = invocation.ArgumentList.Arguments.IndexOf( argument );

                                    if ( symbol is IMethodSymbol invokedMethod && invokedMethod.Parameters[argumentIndex].Type.TypeKind == TypeKind.Delegate )
                                    {
                                        return this.Visit( anonymousFunctionExpression )!;
                                    }
                                }
                                else
                                {
                                    this.RequiresSemanticModel = true;
                                }

                                break;

                            case ObjectCreationExpressionSyntax { ArgumentList: not null } objectCreation:
                                if ( this._semanticModel != null )
                                {
                                    var symbol = this._semanticModel.GetSymbolInfo( objectCreation ).Symbol;
                                    var argumentIndex = objectCreation.ArgumentList.Arguments.IndexOf( argument );

                                    if ( symbol is IMethodSymbol invokedMethod && invokedMethod.Parameters[argumentIndex].Type.TypeKind == TypeKind.Delegate )
                                    {
                                        return this.Visit( anonymousFunctionExpression )!;
                                    }
                                }
                                else
                                {
                                    this.RequiresSemanticModel = true;
                                }

                                break;

                            default:
                                if ( node.Parent.IsKind( SyntaxKind.EqualsValueClause ) )
                                {
                                    return this.Visit( anonymousFunctionExpression )!;
                                }

                                break;
                        }
                    }
                    else if ( node.Parent.IsKind( SyntaxKind.EqualsValueClause ) &&
                              anonymousFunctionExpression is ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 0 } )
                    {
                        return anonymousFunctionExpression;
                    }
                }
            }

            return base.VisitObjectCreationExpression( node )!;
        }
    }
}