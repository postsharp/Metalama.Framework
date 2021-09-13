// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal class MethodLocalDeclarationInliner : MethodInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            // The syntax has to be in form: <type> <local> = <annotated_method_expression>( <arguments> );
            if ( aspectReference.ResolvedSemantic.Symbol is not IMethodSymbol methodSymbol )
            {
                // Coverage: ignore (hit only when the check in base class is incorrect).
                return false;
            }

            // Should be within invocation expression.
            if ( aspectReference.Expression.Parent is not InvocationExpressionSyntax invocationExpression )
            {
                return false;
            }

            // Should be within equals clause.
            if ( invocationExpression.Parent is not EqualsValueClauseSyntax equalsClause )
            {
                return false;
            }

            // Should be within variable declarator.
            if ( equalsClause.Parent is not VariableDeclaratorSyntax variableDeclarator
                || variableDeclarator.Parent is not VariableDeclarationSyntax variableDeclaration )
            {
                // Coverage: ignore (only incorrect code can get here).
                return false;
            }

            // Should be single-variable declaration.
            if ( variableDeclaration.Variables.Count != 1 )
            {
                return false;
            }

            // Variable and method return type should be equal (i.e. no implicit conversions).
            if ( !SymbolEqualityComparer.Default.Equals( semanticModel.GetSymbolInfo( variableDeclaration.Type ).Symbol, methodSymbol.ReturnType ) )
            {
                return false;
            }

            // Should be within local declaration.
            if ( variableDeclaration.Parent == null || variableDeclaration.Parent is not LocalDeclarationStatementSyntax )
            {
                // Coverage: ignore (only incorrect code can get here).
                return false;
            }

            // The invocation needs to be inlineable in itself.
            if ( !IsInlineableInvocation( semanticModel, (IMethodSymbol) aspectReference.ContainingSymbol, invocationExpression ) )
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

            var targetSymbol = (aspectReference.ResolvedSemantic.Symbol as IMethodSymbol).AssertNotNull();

            // Change the target local variable.
            var contextWithLocal = context.WithReturnLocal( targetSymbol, variableDeclarator.Identifier.ValueText );

            // Get the final inlined body of the target method. 
            var inlinedTargetBody = contextWithLocal.GetLinkedBody( targetSymbol.ToSemantic( aspectReference.ResolvedSemantic.Kind ) );

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
                .WithFormattingAnnotationsFrom( localDeclaration )
                .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            replacedNode = localDeclaration;
        }
    }
}