// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal class EventAddAssignmentInliner : PropertyInliner
    {
        public override IReadOnlyList<SyntaxKind> AncestorSyntaxKinds => new[]
        {
            SyntaxKind.ReturnStatement
        };
        
        public override bool CanInline( IMethodSymbol contextDeclaration, SemanticModel semanticModel, ExpressionSyntax annotatedExpression )
        {
            // The syntax needs to be in form: <annotated_property_expression> += value;
            if ( contextDeclaration.AssociatedSymbol is not IEventSymbol )
            {
                return false;
            }

            if ( annotatedExpression.Parent == null || annotatedExpression.Parent is not AssignmentExpressionSyntax assignmentExpression )
            {
                return false;
            }

            // Property access should be on the left.
            if ( assignmentExpression.Left != annotatedExpression )
            {
                return false;
            }

            // Assignment should have a "value" identifier on the right (TODO: ref returns).
            if ( assignmentExpression.Right is not IdentifierNameSyntax rightIdentifier || rightIdentifier.Identifier.ValueText != "value" )
            {
                return false;
            }

            // Assignment should be simple.
            if ( assignmentExpression.Kind() != SyntaxKind.AddAssignmentExpression )
            {
                return false;
            }

            // The assignment should be part of expression statement.
            if ( assignmentExpression.Parent == null || assignmentExpression.Parent is not ExpressionStatementSyntax )
            {
                return false;
            }

            return true;
        }

        public override void Inline( InliningContext context, ExpressionSyntax annotatedExpression, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var semanticModel = context.Compilation.GetSemanticModel( annotatedExpression.SyntaxTree );
            var referencedSymbol = (IEventSymbol)semanticModel.GetSymbolInfo( annotatedExpression ).Symbol.AssertNotNull();
            var assignmentExpression = (AssignmentExpressionSyntax) annotatedExpression.Parent.AssertNotNull();
            var expressionStatement = (ExpressionStatementSyntax) assignmentExpression.Parent.AssertNotNull();

            if ( !annotatedExpression.TryGetAspectReference( out var aspectReference ) )
            {
                throw new AssertionFailedException();
            }

            // We need to change the return variable and label of the inlined code and then generate hand-over code.
            var targetSymbol = (IEventSymbol) context.ReferenceResolver.Resolve( referencedSymbol, aspectReference );

            // Get the final inlined body of the target property setter. 
            var inlinedTargetBody = context.GetLinkedBody( targetSymbol.AddMethod.AssertNotNull() );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );

            newNode = inlinedTargetBody;
            replacedNode = expressionStatement;
        }
    }
}
