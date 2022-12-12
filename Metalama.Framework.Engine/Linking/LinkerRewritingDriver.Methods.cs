// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Substitution;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerRewritingDriver
    {
        public IReadOnlyList<MemberDeclarationSyntax> RewriteMethod(
            MethodDeclarationSyntax methodDeclaration,
            IMethodSymbol symbol,
            SyntaxGenerationContext generationContext )
        {
            if ( this.InjectionRegistry.IsOverrideTarget( symbol ) )
            {
                if ( symbol.IsPartialDefinition && symbol.PartialImplementationPart != null )
                {
                    // This is a partial method declaration that is not to be transformed.
                    return new[] { methodDeclaration };
                }

                var members = new List<MemberDeclarationSyntax>();

                if ( symbol.IsPartialDefinition && symbol.PartialImplementationPart == null )
                {
                    // This is a partial method declaration that did not have any body.
                    // Keep it as is and add a new declaration that will contain the override.
                    members.Add( methodDeclaration );
                }

                var lastOverride = this.InjectionRegistry.GetLastOverride( symbol );

                if ( this.AnalysisRegistry.IsInlined( lastOverride.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final, lastOverride.IsAsync ) );
                }
                else
                {
                    members.Add( GetTrampolineForMethod( methodDeclaration, lastOverride ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    members.Add( GetOriginalImplMethod( methodDeclaration, symbol, generationContext ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) ) )
                {
                    members.Add( GetEmptyImplMethod( methodDeclaration, symbol, generationContext ) );
                }

                return members;
            }
            else if ( this.InjectionRegistry.IsOverride( symbol ) )
            {
                if ( !this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) )
                     || this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[] { GetLinkedDeclaration( IntermediateSymbolSemanticKind.Default, symbol.IsAsync ) };
            }
            else
            {
                throw new AssertionFailedException( $"'{symbol}' is not an override target." );
            }

            MethodDeclarationSyntax GetLinkedDeclaration( IntermediateSymbolSemanticKind semanticKind, bool isAsync )
            {
                var linkedBody = this.GetSubstitutedBody(
                    symbol.ToSemantic( semanticKind ),
                    new SubstitutionContext(
                        this,
                        generationContext,
                        new InliningContextIdentifier( symbol.ToSemantic( semanticKind ) ) ) );

                var modifiers = methodDeclaration.Modifiers;

                if ( isAsync && !symbol.IsAsync )
                {
                    modifiers = modifiers.Add( Token( TriviaList( ElasticSpace ), SyntaxKind.AsyncKeyword, TriviaList( ElasticSpace ) ) );
                }
                else if ( !isAsync && symbol.IsAsync )
                {
                    modifiers = TokenList( modifiers.Where( m => !m.IsKind( SyntaxKind.AsyncKeyword ) ) );
                }

                // Trivia processing:
                //   * For block bodies methods, we preserve trivia of the opening/closing brace.
                //   * For expression bodied methods:
                //       int Foo() <trivia_leading_equals_value> => <trivia_trailing_equals_value> <expression> <trivia_leading_semicolon> ; <trivia_trailing_semicolon>
                //       int Foo() <trivia_leading_equals_value> { <trivia_trailing_equals_value> <linked_body> <trivia_leading_semicolon> } <trivia_trailing_semicolon>

                var (openBraceLeadingTrivia, openBraceTrailingTrivia, closeBraceLeadingTrivia, closeBraceTrailingTrivia) =
                    methodDeclaration switch
                    {
                        { Body: { OpenBraceToken: var openBraceToken, CloseBraceToken: var closeBraceToken } } =>
                            (openBraceToken.LeadingTrivia, openBraceToken.TrailingTrivia, closeBraceToken.LeadingTrivia, closeBraceToken.TrailingTrivia),
                        { ExpressionBody: { ArrowToken: var arrowToken }, SemicolonToken: var semicolonToken } =>
                            (arrowToken.LeadingTrivia.Add( ElasticLineFeed ), arrowToken.TrailingTrivia.Add( ElasticLineFeed ),
                             semicolonToken.LeadingTrivia.Add( ElasticLineFeed ), semicolonToken.TrailingTrivia),
                        { Body: null, ExpressionBody: null, SemicolonToken: var semicolonToken } =>
                            (semicolonToken.LeadingTrivia.Add( ElasticLineFeed ), TriviaList( ElasticLineFeed ), TriviaList( ElasticLineFeed ),
                             semicolonToken.TrailingTrivia),
                        _ => throw new AssertionFailedException( $"Unexpected method declaration at '{methodDeclaration.GetLocation()}'." )
                    };

                var ret = methodDeclaration
                    .WithExpressionBody( null )
                    .WithModifiers( modifiers )
                    .WithBody(
                        Block( linkedBody )
                            .WithOpenBraceToken( Token( openBraceLeadingTrivia, SyntaxKind.OpenBraceToken, openBraceTrailingTrivia ) )
                            .WithCloseBraceToken( Token( closeBraceLeadingTrivia, SyntaxKind.CloseBraceToken, closeBraceTrailingTrivia ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock )
                            .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )
                    .WithSemicolonToken( default );

                return ret;
            }
        }

        private static MemberDeclarationSyntax GetOriginalImplMethod(
            MethodDeclarationSyntax method,
            IMethodSymbol symbol,
            SyntaxGenerationContext generationContext )
            => GetSpecialImplMethod(
                method,
                method.Body.WithSourceCodeAnnotation(),
                method.ExpressionBody.WithSourceCodeAnnotation(),
                symbol,
                GetOriginalImplMemberName( symbol ),
                generationContext );

        private static MemberDeclarationSyntax GetEmptyImplMethod(
            MethodDeclarationSyntax method,
            IMethodSymbol symbol,
            SyntaxGenerationContext generationContext )
        {
            var emptyBody =
                symbol.ReturnsVoid
                    ? SyntaxFactoryEx.FormattedBlock()
                    : SyntaxFactoryEx.FormattedBlock(
                        ReturnStatement(
                            Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Space ),
                            DefaultExpression( method.ReturnType ),
                            Token( SyntaxKind.SemicolonToken ) ) );

            return GetSpecialImplMethod( method, emptyBody, null, symbol, GetEmptyImplMemberName( symbol ), generationContext );
        }

        private static MemberDeclarationSyntax GetSpecialImplMethod(
            MethodDeclarationSyntax method,
            BlockSyntax? body,
            ArrowExpressionClauseSyntax? expressionBody,
            IMethodSymbol symbol,
            string name,
            SyntaxGenerationContext generationContext )
        {
            var returnType = AsyncHelper.GetIntermediateMethodReturnType( symbol, method.ReturnType, generationContext );

            var modifiers = symbol
                .GetSyntaxModifierList( ModifierCategories.Static | ModifierCategories.Unsafe | ModifierCategories.Async )
                .Insert( 0, Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ) );

            var constraints = method.ConstraintClauses;

            if ( constraints.Count == 0 && symbol.OverriddenMethod != null )
            {
                // Constraints may be inherited from the overridden method.

                constraints = generationContext.SyntaxGenerator.TypeParameterConstraintClauses( symbol.TypeParameters );
            }

            return
                MethodDeclaration(
                        List<AttributeListSyntax>(),
                        modifiers,
                        returnType.WithTrailingTrivia( Space ),
                        null,
                        Identifier( name ),
                        method.TypeParameterList != null ? FilterAttributesOnSpecialImpl( method.TypeParameterList ) : null,
                        FilterAttributesOnSpecialImpl( method.ParameterList ),
                        constraints,
                        null,
                        null )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithBody( body )
                    .WithExpressionBody( expressionBody )
                    .WithSemicolonToken( expressionBody != null ? Token( SyntaxKind.SemicolonToken ) : default )
                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );
        }

        private static MethodDeclarationSyntax GetTrampolineForMethod( MethodDeclarationSyntax method, IMethodSymbol targetSymbol )
        {
            // TODO: First override not being inlineable probably does not happen outside of specifically written linker tests, i.e. trampolines may not be needed.

            return method
                .WithBody( GetBody() )
                .WithModifiers( TokenList( method.Modifiers.Where( m => !m.IsKind( SyntaxKind.AsyncKeyword ) ) ) )
                .NormalizeWhitespace()
                .WithLeadingTrivia( method.GetLeadingTrivia() )
                .WithTrailingTrivia( method.GetTrailingTrivia() );

            BlockSyntax GetBody()
            {
                var invocation =
                    InvocationExpression(
                        GetInvocationTarget(),
                        ArgumentList(
                            SeparatedList( method.ParameterList.Parameters.SelectAsEnumerable( x => Argument( IdentifierName( x.Identifier ) ) ) ) ) );

                if ( !targetSymbol.ReturnsVoid )
                {
                    return SyntaxFactoryEx.FormattedBlock(
                        ReturnStatement(
                            Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                            invocation,
                            Token( SyntaxKind.SemicolonToken ) ) );
                }
                else
                {
                    return SyntaxFactoryEx.FormattedBlock( ExpressionStatement( invocation ) );
                }

                ExpressionSyntax GetInvocationTarget()
                {
                    if ( targetSymbol.IsStatic )
                    {
                        return IdentifierName( targetSymbol.Name );
                    }
                    else
                    {
                        return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( targetSymbol.Name ) );
                    }
                }
            }
        }
    }
}