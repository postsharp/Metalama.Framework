// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal class PropertyGetReturnInliner : PropertyInliner
    {
        public override IReadOnlyList<SyntaxKind> AncestorSyntaxKinds => new[]
        {
            SyntaxKind.ReturnStatement
        };
        
        public override bool CanInline( ISymbol contextDeclaration, SemanticModel semanticModel, ExpressionSyntax annotatedExpression )
        {
            // The syntax needs to be in form: return <annotated_property_expression>;

            if ( !annotatedExpression.TryGetAspectReference( out _ ) )
            {
                throw new AssertionFailedException();
            }

            if ( contextDeclaration is not IPropertySymbol )
            {
                throw new AssertionFailedException();
            }

            if ( annotatedExpression.Parent == null || annotatedExpression.Parent is not ReturnStatementSyntax )
            {
                return false;
            }

            return true;
        }

        public override void Inline( InliningContext context, ExpressionSyntax annotatedExpression, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var semanticModel = context.Compilation.GetSemanticModel( annotatedExpression.SyntaxTree );
            var referencedSymbol = (IPropertySymbol) semanticModel.GetSymbolInfo( annotatedExpression ).Symbol.AssertNotNull();
            var expressionStatement = (ReturnStatementSyntax) annotatedExpression.Parent.AssertNotNull();

            if ( !annotatedExpression.TryGetAspectReference( out var aspectReference ) )
            {
                throw new AssertionFailedException();
            }

            // We need to change the return variable and label of the inlined code and then generate hand-over code.
            var targetSymbol = (IPropertySymbol)context.ReferenceResolver.Resolve( referencedSymbol, aspectReference );

            // Get the final inlined body of the target property getter. 
            var inlinedTargetBody = context.GetLinkedBody( targetSymbol.GetMethod.AssertNotNull() );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );

            newNode = inlinedTargetBody;
            replacedNode = expressionStatement;
        }
    }
}
