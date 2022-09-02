﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal class PropertyGetCastReturnInliner : PropertyGetInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            if ( !base.CanInline( aspectReference, semanticModel ) )
            {
                return false;
            }

            // The syntax needs to be in form: return <annotated_property_expression>;
            if ( aspectReference.ResolvedSemantic.Symbol is not IPropertySymbol
                 && (aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol is not IPropertySymbol )
            {
                // Coverage: ignore (hit only when the check in base class is incorrect).
                return false;
            }

            if ( aspectReference.Expression.Parent == null || aspectReference.Expression.Parent is not CastExpressionSyntax castExpression )
            {
                return false;
            }

            if ( !SymbolEqualityComparer.Default.Equals(
                    semanticModel.GetSymbolInfo( castExpression.Type ).Symbol,
                    ((IMethodSymbol) aspectReference.ContainingSymbol).ReturnType ) )
            {
                return false;
            }

            if ( castExpression.Parent == null || castExpression.Parent is not ReturnStatementSyntax )
            {
                return false;
            }

            return true;
        }

        public override void Inline( InliningContext context, ResolvedAspectReference aspectReference, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var castExpression = (CastExpressionSyntax) aspectReference.Expression.Parent.AssertNotNull();
            var returnStatement = (ReturnStatementSyntax) castExpression.Parent.AssertNotNull();

            var targetSymbol =
                aspectReference.ResolvedSemantic.Symbol as IPropertySymbol
                ?? (IPropertySymbol) ((aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol).AssertNotNull();

            // Get the final inlined body of the target property getter. 
            var inlinedTargetBody = context.GetLinkedBody( targetSymbol.GetMethod.AssertNotNull().ToSemantic( aspectReference.ResolvedSemantic.Kind ) );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            newNode = inlinedTargetBody;
            replacedNode = returnStatement;
        }
    }
}