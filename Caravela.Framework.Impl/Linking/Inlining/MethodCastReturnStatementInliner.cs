﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    /// <summary>
    /// Handles inlining of return statement which invokes an annotated expression.
    /// </summary>
    internal class MethodCastReturnStatementInliner : MethodInliner
    {
        public override IReadOnlyList<SyntaxKind> AncestorSyntaxKinds => new[]
        {
            SyntaxKind.ReturnStatement
        };
        
        public override bool CanInline( IMethodSymbol contextDeclaration, SemanticModel semanticModel, ExpressionSyntax annotatedExpression )
        {
            // The syntax has to be in form: return <annotated_method_expression( <arguments> );
            if ( annotatedExpression.Parent == null || annotatedExpression.Parent is not InvocationExpressionSyntax invocationExpression)
            {
                return false;
            }

            if ( invocationExpression.Parent == null || invocationExpression.Parent is not CastExpressionSyntax castExpression )
            {
                return false;
            }

            if ( !SymbolEqualityComparer.Default.Equals( semanticModel.GetSymbolInfo(castExpression.Type).Symbol, contextDeclaration.ReturnType) )
            {
                return false;
            }

            if ( castExpression.Parent == null || castExpression.Parent is not ReturnStatementSyntax)
            {
                return false;
            }

            if (invocationExpression.ArgumentList.Arguments.Count != contextDeclaration.Parameters.Length)
            {
                return false;
            }

            return true;
        }

        public override void Inline( InliningContext context, ExpressionSyntax annotatedExpression, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var semanticModel = context.Compilation.GetSemanticModel( annotatedExpression.SyntaxTree );
            var referencedSymbol = (IMethodSymbol) semanticModel.GetSymbolInfo( annotatedExpression ).Symbol.AssertNotNull();
            var invocationExpression = (InvocationExpressionSyntax)annotatedExpression.Parent.AssertNotNull();
            var castExpression = (CastExpressionSyntax) invocationExpression.Parent.AssertNotNull();
            var returnStatement = (ReturnStatementSyntax) castExpression.Parent.AssertNotNull();

            // Regardless of whether we are returning directly or through variable+label, we just ask for the target method body a return it.
            if ( !annotatedExpression.TryGetAspectReference( out var aspectReference ) )
            {
                throw new AssertionFailedException();
            }

            var targetSymbol = (IMethodSymbol) context.ReferenceResolver.Resolve( referencedSymbol, aspectReference );

            // Get the final body (after inlinings) of the target.
            var inlinedTargetBody = context.GetLinkedBody( targetSymbol );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );

            replacedNode = returnStatement;
            newNode = inlinedTargetBody;
        }
    }
}
