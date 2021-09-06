// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal class PropertyGetLocalDeclarationInliner : PropertyGetInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            // The syntax has to be in form: <type> <local> = <annotated_property_expression>;
            if ( aspectReference.ResolvedSemantic.Symbol is not IPropertySymbol
                 && (aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol is not IPropertySymbol )
            {
                return false;
            }

            if ( aspectReference.Expression.Parent == null || aspectReference.Expression.Parent is not EqualsValueClauseSyntax equalsClause )
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

            return true;
        }

        public override void Inline( InliningContext context, ResolvedAspectReference aspectReference, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var equalsClause = (EqualsValueClauseSyntax) aspectReference.Expression.Parent.AssertNotNull();
            var variableDeclarator = (VariableDeclaratorSyntax) equalsClause.Parent.AssertNotNull();
            var variableDeclaration = (VariableDeclarationSyntax) variableDeclarator.Parent.AssertNotNull();
            var localDeclaration = (LocalDeclarationStatementSyntax) variableDeclaration.Parent.AssertNotNull();

            var targetSymbol =
                aspectReference.ResolvedSemantic.Symbol as IPropertySymbol
                ?? (IPropertySymbol) ((aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol).AssertNotNull();

            // Change the target local variable.
            var contextWithLocal = context.WithReturnLocal( targetSymbol.GetMethod.AssertNotNull(), variableDeclarator.Identifier.ValueText );

            // Get the final inlined body of the target method. 
            var inlinedTargetBody = contextWithLocal.GetLinkedBody( targetSymbol.GetMethod.AssertNotNull().ToSemantic( aspectReference.ResolvedSemantic.Kind ) );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            // We're replacing the whole return statement.
            newNode =
                Block(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( targetSymbol.Type ).WithTrailingTrivia( Whitespace( " " ) ),
                                SingletonSeparatedList( VariableDeclarator( variableDeclarator.Identifier ) ) ) ),
                        inlinedTargetBody )
                    .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            replacedNode = localDeclaration;
        }
    }
}