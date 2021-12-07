// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.Formatting;
using Metalama.Framework.Impl.Linking.Inlining;
using Metalama.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Impl.Linking
{
    internal partial class LinkerRewritingDriver
    {
        public IReadOnlyList<MemberDeclarationSyntax> RewriteMethod(
            MethodDeclarationSyntax methodDeclaration,
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
                    members.Add( GetTrampolineMethod( methodDeclaration, lastOverride ) );
                }

                if ( this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ) )
                     && !this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ), out _ ) )
                {
                    members.Add( GetOriginalImplMethod( methodDeclaration, symbol, generationContext ) );
                }

                if ( this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Base ) )
                     && !this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Base ), out _ ) )
                {
                    members.Add( GetEmptyImplMethod( methodDeclaration, symbol, generationContext ) );
                }

                return members;
            }
            else if ( this._introductionRegistry.IsOverride( symbol ) )
            {
                if ( !this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ) )
                     || this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ), out _ ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[] { GetLinkedDeclaration( IntermediateSymbolSemanticKind.Default, symbol.IsAsync ) };
            }
            else
            {
                throw new AssertionFailedException();
            }

            MethodDeclarationSyntax GetLinkedDeclaration( IntermediateSymbolSemanticKind semanticKind, bool isAsync )
            {
                var linkedBody = this.GetLinkedBody(
                    symbol.ToSemantic( semanticKind ),
                    InliningContext.Create( this, symbol, generationContext ) );

                var modifiers = methodDeclaration.Modifiers;

                if ( isAsync && !symbol.IsAsync )
                {
                    modifiers = modifiers.Add( Token( TriviaList( ElasticSpace ), SyntaxKind.AsyncKeyword, TriviaList( ElasticSpace ) ) );
                }
                else if ( !isAsync && symbol.IsAsync )
                {
                    modifiers = TokenList( modifiers.Where( m => m.Kind() != SyntaxKind.AsyncKeyword ) );
                }

                return methodDeclaration
                    .WithExpressionBody( null )
                    .WithModifiers( modifiers )
                    .WithBody( linkedBody )
                    .WithLeadingTrivia( methodDeclaration.GetLeadingTrivia() )
                    .WithTrailingTrivia( methodDeclaration.GetTrailingTrivia() );
            }
        }

        private static MemberDeclarationSyntax GetOriginalImplMethod(
            MethodDeclarationSyntax method,
            IMethodSymbol symbol,
            SyntaxGenerationContext generationContext )
            => GetSpecialImplMethod(
                method,
                method.Body.AddSourceCodeAnnotation(),
                method.ExpressionBody.AddSourceCodeAnnotation(),
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
                    ? Block()
                    : Block( ReturnStatement( DefaultExpression( method.ReturnType ) ) ).NormalizeWhitespace();

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
                .Insert( 0, Token( SyntaxKind.PrivateKeyword ) );

            return
                MethodDeclaration(
                        List<AttributeListSyntax>(),
                        modifiers,
                        returnType,
                        null,
                        Identifier( name ),
                        method.TypeParameterList,
                        method.ParameterList,
                        method.ConstraintClauses,
                        null,
                        null )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithBody( body )
                    .WithExpressionBody( expressionBody )
                    .AddGeneratedCodeAnnotation();
        }
    }
}