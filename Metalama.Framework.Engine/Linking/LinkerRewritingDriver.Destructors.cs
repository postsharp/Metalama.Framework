﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Inlining;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerRewritingDriver
    {
        // Destructors/finalizers are only override targets, overrides are always represented as methods.

        public IReadOnlyList<MemberDeclarationSyntax> RewriteDestructor(
            DestructorDeclarationSyntax destructorDeclaration,
            IMethodSymbol symbol,
            SyntaxGenerationContext generationContext )
        {
            if ( this._introductionRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IMethodSymbol) this._introductionRegistry.GetLastOverride( symbol );

                if ( this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( lastOverride, IntermediateSymbolSemanticKind.Default ), out _ ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final, lastOverride.IsAsync ) );
                }
                else
                {
                    members.Add( GetTrampolineDestructor( destructorDeclaration, lastOverride ) );
                }

                if ( this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ) )
                     && !this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ), out _ ) )
                {
                    members.Add( GetOriginalImplDestructor( destructorDeclaration, symbol ) );
                }

                if ( this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Base ) )
                     && !this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Base ), out _ ) )
                {
                    members.Add( GetEmptyImplDestructor( destructorDeclaration, symbol ) );
                }

                return members;
            }
            else
            {
                throw new AssertionFailedException();
            }

            DestructorDeclarationSyntax GetLinkedDeclaration( IntermediateSymbolSemanticKind semanticKind, bool isAsync )
            {
                var linkedBody = this.GetLinkedBody(
                    symbol.ToSemantic( semanticKind ),
                    InliningContext.Create( this, symbol, generationContext ) );

                var modifiers = destructorDeclaration.Modifiers;

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
                    destructorDeclaration switch
                    {
                        { Body: { OpenBraceToken: var openBraceToken, CloseBraceToken: var closeBraceToken } } =>
                            (openBraceToken.LeadingTrivia, openBraceToken.TrailingTrivia, closeBraceToken.LeadingTrivia, closeBraceToken.TrailingTrivia),
                        { ExpressionBody: { ArrowToken: var arrowToken }, SemicolonToken: var semicolonToken } =>
                            (arrowToken.LeadingTrivia.Add( ElasticLineFeed ), arrowToken.TrailingTrivia.Add( ElasticLineFeed ),
                             semicolonToken.LeadingTrivia.Add( ElasticLineFeed ), semicolonToken.TrailingTrivia),
                        _ => throw new AssertionFailedException()
                    };

                var ret = destructorDeclaration
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

        private static MemberDeclarationSyntax GetOriginalImplDestructor(
            DestructorDeclarationSyntax destructor,
            IMethodSymbol symbol )
            => GetSpecialImplDestructor(
                destructor,
                destructor.Body.WithSourceCodeAnnotation(),
                destructor.ExpressionBody.WithSourceCodeAnnotation(),
                symbol,
                GetOriginalImplMemberName( symbol ) );

        private static MemberDeclarationSyntax GetEmptyImplDestructor(
            DestructorDeclarationSyntax destructor,
            IMethodSymbol symbol )
        {
            var emptyBody = Block().NormalizeWhitespace();

            return GetSpecialImplDestructor( destructor, emptyBody, null, symbol, GetEmptyImplMemberName( symbol ) );
        }

        private static MemberDeclarationSyntax GetSpecialImplDestructor(
            DestructorDeclarationSyntax destructor,
            BlockSyntax? body,
            ArrowExpressionClauseSyntax? expressionBody,
            IMethodSymbol symbol,
            string name )
        {
            var modifiers = symbol
                .GetSyntaxModifierList( ModifierCategories.Static | ModifierCategories.Unsafe | ModifierCategories.Async )
                .Insert( 0, Token( SyntaxKind.PrivateKeyword ) );

            return
                MethodDeclaration(
                        List<AttributeListSyntax>(),
                        modifiers,
                        PredefinedType( Token( SyntaxKind.VoidKeyword ) ),
                        null,
                        Identifier( name ),
                        null,
                        destructor.ParameterList,
                        List<TypeParameterConstraintClauseSyntax>(),
                        null,
                        null )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithBody( body )
                    .WithExpressionBody( expressionBody )
                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );
        }
    }
}