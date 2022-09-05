﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    /// <summary>
    /// Handles inlining of return statement which invokes an annotated expression.
    /// </summary>
    internal class MethodCastReturnStatementInliner : MethodInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            if ( !base.CanInline( aspectReference, semanticModel ) )
            {
                return false;
            }

            // The syntax has to be in form: return (<type>)<annotated_method_expression( <arguments> );
            if ( aspectReference.ResolvedSemantic.Symbol is not IMethodSymbol )
            {
                // Coverage: ignore (hit only when the check in base class is incorrect).
                return false;
            }

            if ( aspectReference.SourceExpression.AssertNotNull().Parent == null || aspectReference.SourceExpression.AssertNotNull().Parent is not InvocationExpressionSyntax invocationExpression )
            {
                return false;
            }

            if ( invocationExpression.Parent == null || invocationExpression.Parent is not CastExpressionSyntax castExpression )
            {
                return false;
            }

            if ( !SignatureTypeSymbolComparer.Instance.Equals(
                    semanticModel.GetSymbolInfo( castExpression.Type ).Symbol,
                    aspectReference.ContainingSemantic.Symbol.ReturnType ) )
            {
                return false;
            }

            if ( castExpression.Parent is not ReturnStatementSyntax )
            {
                return false;
            }

            // The invocation needs to be inlineable in itself.
            if ( !IsInlineableInvocation( semanticModel, aspectReference.ContainingSemantic.Symbol, invocationExpression ) )
            {
                return false;
            }

            return true;
        }

        public override InliningAnalysisInfo GetInliningAnalysisInfo( InliningAnalysisContext context, ResolvedAspectReference aspectReference )
        {
            var invocationExpression = (InvocationExpressionSyntax) aspectReference.SourceExpression.AssertNotNull().Parent.AssertNotNull();
            var castExpression = (CastExpressionSyntax) invocationExpression.Parent.AssertNotNull();
            var returnStatement = (ReturnStatementSyntax) castExpression.Parent.AssertNotNull();

            return new InliningAnalysisInfo( returnStatement, null );
        }

        public override StatementSyntax Inline( SyntaxGenerationContext syntaxGenerationContext, InliningSpecification specification, SyntaxNode currentNode, StatementSyntax linkedTargetBody )
        {
            return linkedTargetBody;
        }
    }
}