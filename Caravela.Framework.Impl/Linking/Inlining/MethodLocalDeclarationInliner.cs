// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal class MethodLocalDeclarationInliner : MethodInliner
    {
        public override IReadOnlyList<SyntaxKind> AncestorSyntaxKinds => new[] { SyntaxKind.ReturnStatement };

        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            // The syntax has to be in form: <type> <local> = <annotated_method_expression>( <arguments> );
            if ( aspectReference.ResolvedSymbol is not IMethodSymbol )
            {
                return false;
            }

            if ( aspectReference.Expression.Parent == null || aspectReference.Expression.Parent is not InvocationExpressionSyntax invocationExpression )
            {
                return false;
            }

            if ( invocationExpression.Parent == null || invocationExpression.Parent is not EqualsValueClauseSyntax equalsClause )
            {
                return false;
            }

            if ( equalsClause.Parent == null || equalsClause.Parent is not VariableDeclaratorSyntax variableDeclarator )
            {
                return false;
            }

            if ( variableDeclarator.Parent == null || variableDeclarator.Parent is not VariableDeclarationSyntax variableDeclaration )
            {
                return false;
            }

            if ( variableDeclaration.Variables.Count != 1 )
            {
                return false;
            }

            if ( variableDeclaration.Parent == null || variableDeclaration.Parent is not LocalDeclarationStatementSyntax _ )
            {
                return false;
            }

            // The invocation needs to be inlineable in itself.
            if ( IsInlineableInvocation( semanticModel, (IMethodSymbol) aspectReference.ContainingSymbol, invocationExpression ) )
            {
                return false;
            }

            return true;
        }

        public override void Inline( InliningContext context, ResolvedAspectReference aspectReference, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var invocationExpression = (InvocationExpressionSyntax) aspectReference.Expression.Parent.AssertNotNull();
            var equalsClause = (EqualsValueClauseSyntax) invocationExpression.Parent.AssertNotNull();
            var variableDeclarator = (VariableDeclaratorSyntax) equalsClause.Parent.AssertNotNull();
            var variableDeclaration = (VariableDeclarationSyntax) variableDeclarator.Parent.AssertNotNull();
            var localDeclaration = (LocalDeclarationStatementSyntax) variableDeclaration.Parent.AssertNotNull();

            var targetSymbol = (aspectReference.ResolvedSymbol as IMethodSymbol).AssertNotNull();

            // Change the target local variable.
            var contextWithLocal = context.WithReturnLocal( targetSymbol, variableDeclarator.Identifier.ValueText );

            // Get the final inlined body of the target method. 
            var inlinedTargetBody = contextWithLocal.GetLinkedBody( targetSymbol );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            // We're replacing the whole return statement.
            newNode = Block(
                    LocalDeclarationStatement(
                            VariableDeclaration(
                                LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( targetSymbol.ReturnType ),
                                SingletonSeparatedList( VariableDeclarator( variableDeclarator.Identifier ) ) ) )
                        .NormalizeWhitespace()
                        .WithTrailingTrivia( ElasticLineFeed ),
                    inlinedTargetBody )
                .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            replacedNode = localDeclaration;
        }
    }
}