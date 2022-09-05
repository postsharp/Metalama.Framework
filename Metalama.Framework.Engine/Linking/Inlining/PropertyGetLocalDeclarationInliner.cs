﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal class PropertyGetLocalDeclarationInliner : PropertyGetInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            if ( !base.CanInline( aspectReference, semanticModel ) )
            {
                return false;
            }

            // The syntax has to be in form: <type> <local> = <annotated_property_expression>;
            if ( aspectReference.ResolvedSemantic.Symbol is not IPropertySymbol
                 && (aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol is not IPropertySymbol )
            {
                // Coverage: ignore (hit only when the check in base class is incorrect).
                return false;
            }

            var propertySymbol =
                aspectReference.ResolvedSemantic.Symbol as IPropertySymbol
                ?? (IPropertySymbol) ((aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol).AssertNotNull();

            // Should be within equals clause.
            if ( aspectReference.SourceExpression.Parent == null || aspectReference.SourceExpression.Parent is not EqualsValueClauseSyntax equalsClause )
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

            // Variable and property type should be equal (i.e. no implicit conversions).
            if ( !SymbolEqualityComparer.Default.Equals( semanticModel.GetSymbolInfo( variableDeclaration.Type ).Symbol, propertySymbol.Type ) )
            {
                return false;
            }

            // Should be within local declaration.
            if ( variableDeclaration.Parent == null || variableDeclaration.Parent is not LocalDeclarationStatementSyntax )
            {
                // Coverage: ignore (only incorrect code can get here).
                return false;
            }

            return true;
        }

        public override InliningAnalysisInfo GetInliningAnalysisInfo( InliningAnalysisContext context, ResolvedAspectReference aspectReference )
        {
            var equalsClause = (EqualsValueClauseSyntax) aspectReference.SourceExpression.Parent.AssertNotNull();
            var variableDeclarator = (VariableDeclaratorSyntax) equalsClause.Parent.AssertNotNull();
            var variableDeclaration = (VariableDeclarationSyntax) variableDeclarator.Parent.AssertNotNull();
            var localDeclaration = (LocalDeclarationStatementSyntax) variableDeclaration.Parent.AssertNotNull();

            return new InliningAnalysisInfo( localDeclaration, variableDeclarator.Identifier.Text );
        }

        public override StatementSyntax Inline( SyntaxGenerationContext syntaxGenerationContext, InliningSpecification specification, SyntaxNode currentNode, StatementSyntax linkedTargetBody )
        {
            if ( currentNode is not StatementSyntax currentStatement )
            {
                throw new AssertionFailedException();
            }

            return Block(
                    LocalDeclarationStatement(
                            VariableDeclaration(
                                syntaxGenerationContext.SyntaxGenerator.Type( specification.DestinationSemantic.Symbol.ReturnType ),
                                SingletonSeparatedList( VariableDeclarator( Identifier( specification.ReturnVariableIdentifier.AssertNotNull() ) ) ) ) )
                        .NormalizeWhitespace()
                        .WithTrailingTrivia( ElasticLineFeed ),
                    linkedTargetBody )
                .WithFormattingAnnotationsFrom( currentStatement )
                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
        }
    }
}