// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Metalama.Framework.Engine.Formatting;

public partial class CodeFormatter
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

        public override SyntaxNode VisitObjectCreationExpression( ObjectCreationExpressionSyntax node )
        {
            // We want to simplify delegate creation from `new Action( () => { ... } )` to just `() => { ... }`.
            // This simplification is valid when the delegate is target typed. It is implemented for invocation arguments
            // and assignments.
            if ( node.HasAnnotation( Simplifier.Annotation ) && node.ArgumentList?.Arguments is
                                                                 [{ Expression: AnonymousFunctionExpressionSyntax anonymousFunctionExpression }]
                                                             && node.Parent?.Kind() is SyntaxKind.Argument or SyntaxKind.SimpleAssignmentExpression )
            {
                if ( node.Parent is ArgumentSyntax argument && node.Parent.Parent?.Parent is InvocationExpressionSyntax invocation )
                {
                    if ( this._semanticModel != null )
                    {
                        var symbol = this._semanticModel.GetSymbolInfo( invocation.Expression ).Symbol;
                        var argumentIndex = invocation.ArgumentList.Arguments.IndexOf( argument );

                        if ( symbol is IMethodSymbol invokedMethod && invokedMethod.Parameters[argumentIndex].Type.TypeKind == TypeKind.Delegate )
                        {
                            return anonymousFunctionExpression;
                        }
                    }
                    else
                    {
                        this.RequiresSemanticModel = true;
                    }
                }
                else if ( node.Parent.IsKind( SyntaxKind.SimpleAssignmentExpression ) )
                {
                    return anonymousFunctionExpression;
                }
            }

            return node;
        }
    }
}