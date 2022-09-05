﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
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

            if ( aspectReference.SourceExpression.AssertNotNull().Parent == null || aspectReference.SourceExpression.AssertNotNull().Parent is not CastExpressionSyntax castExpression )
            {
                return false;
            }

            if ( !SymbolEqualityComparer.Default.Equals(
                    semanticModel.GetSymbolInfo( castExpression.Type ).Symbol,
                    aspectReference.ContainingSemantic.Symbol.ReturnType ) )
            {
                return false;
            }

            if ( castExpression.Parent == null || castExpression.Parent is not ReturnStatementSyntax )
            {
                return false;
            }

            return true;
        }

        public override InliningAnalysisInfo GetInliningAnalysisInfo( InliningAnalysisContext context, ResolvedAspectReference aspectReference )
        {
            var castExpression = (CastExpressionSyntax) aspectReference.SourceExpression.AssertNotNull().Parent.AssertNotNull();
            var returnStatement = (ReturnStatementSyntax) castExpression.Parent.AssertNotNull();

            return new InliningAnalysisInfo( returnStatement, null );
        }

        public override StatementSyntax Inline( SyntaxGenerationContext syntaxGenerationContext, InliningSpecification specification, SyntaxNode currentNode, StatementSyntax linkedTargetBody )
        {
            return linkedTargetBody;
        }
    }
}