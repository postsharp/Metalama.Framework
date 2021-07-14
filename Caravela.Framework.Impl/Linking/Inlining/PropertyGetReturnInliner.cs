// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal class PropertyGetReturnInliner : PropertyInliner
    {
        public override IReadOnlyList<SyntaxKind> AncestorSyntaxKinds => new[] { SyntaxKind.ReturnStatement };

        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            // The syntax needs to be in form: return <annotated_property_expression>;
            if ( aspectReference.ResolvedSymbol is not IPropertySymbol
                 && (aspectReference.ResolvedSymbol as IMethodSymbol)?.AssociatedSymbol is not IPropertySymbol )
            {
                return false;
            }

            if ( aspectReference.Expression.Parent == null || aspectReference.Expression.Parent is not ReturnStatementSyntax )
            {
                return false;
            }

            return true;
        }

        public override void Inline( InliningContext context, ResolvedAspectReference aspectReference, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var expressionStatement = (ReturnStatementSyntax) aspectReference.Expression.Parent.AssertNotNull();

            var targetSymbol =
                aspectReference.ResolvedSymbol as IPropertySymbol
                ?? (IPropertySymbol) ((aspectReference.ResolvedSymbol as IMethodSymbol)?.AssociatedSymbol).AssertNotNull();

            // Get the final inlined body of the target property getter. 
            var inlinedTargetBody = context.GetLinkedBody( targetSymbol.GetMethod.AssertNotNull() );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );

            newNode = inlinedTargetBody;
            replacedNode = expressionStatement;
        }
    }
}