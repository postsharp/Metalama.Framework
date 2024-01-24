// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Substitution;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LinkerRewritingDriver
    {
        // Destructors/finalizers are only override targets, overrides are always represented as methods.

        private IReadOnlyList<MemberDeclarationSyntax> RewriteConstructor(
            ConstructorDeclarationSyntax constructorDeclaration,
            IMethodSymbol symbol,
            SyntaxGenerationContext generationContext )
        {
            if ( this.AnalysisRegistry.HasAnySubstitutions( symbol ) )
            {
                return new[] { GetLinkedDeclaration() };
            }
            else
            {
                return new[] { constructorDeclaration };
            }

            ConstructorDeclarationSyntax GetLinkedDeclaration()
            {
                var linkedBody = this.GetSubstitutedBody(
                    symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ),
                    new SubstitutionContext(
                        this,
                        generationContext,
                        new InliningContextIdentifier( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) ) );

                var (openBraceLeadingTrivia, openBraceTrailingTrivia, closeBraceLeadingTrivia, closeBraceTrailingTrivia) =
                    constructorDeclaration switch
                    {
                        { Body: { OpenBraceToken: var openBraceToken, CloseBraceToken: var closeBraceToken } } =>
                            (openBraceToken.LeadingTrivia, openBraceToken.TrailingTrivia, closeBraceToken.LeadingTrivia, closeBraceToken.TrailingTrivia),
                        { ExpressionBody.ArrowToken: var arrowToken, SemicolonToken: var semicolonToken } =>
                            (arrowToken.LeadingTrivia.Add( ElasticLineFeed ), arrowToken.TrailingTrivia.Add( ElasticLineFeed ),
                             semicolonToken.LeadingTrivia.Add( ElasticLineFeed ), semicolonToken.TrailingTrivia),
                        _ => throw new AssertionFailedException( $"Unsupported form of constructor declaration for {symbol}." )
                    };

                var ret = constructorDeclaration.PartialUpdate(
                    expressionBody: null,
                    body: Block(
                            Token( openBraceLeadingTrivia, SyntaxKind.OpenBraceToken, openBraceTrailingTrivia ),
                            SingletonList<StatementSyntax>( linkedBody ),
                            Token( closeBraceLeadingTrivia, SyntaxKind.CloseBraceToken, closeBraceTrailingTrivia ) )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock )
                        .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ),
                    semicolonToken: default );

                return ret;
            }
        }
    }
}