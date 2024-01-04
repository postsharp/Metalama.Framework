// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Substitution;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// Properties with overrides have the following structure:
//  * Final semantic. 
//  * Override n
//  * ...
//  * Override 1
//  * Default semantic.
//  * Base semantic (if the property was introduced).

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LinkerRewritingDriver
    {
        private IReadOnlyList<MemberDeclarationSyntax> RewriteIndexer(
            IndexerDeclarationSyntax indexerDeclaration,
            IPropertySymbol symbol,
            SyntaxGenerationContext generationContext )
        {
            if ( this.InjectionRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IPropertySymbol) this.InjectionRegistry.GetLastOverride( symbol );

                if ( this.AnalysisRegistry.IsInlined( lastOverride.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final ) );
                }
                else
                {
                    members.Add( GetTrampolineForIndexer( indexerDeclaration, lastOverride ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) )
                     && this.ShouldGenerateSourceMember( symbol ) )
                {
                    members.Add(
                        this.GetOriginalImplIndexer(
                            indexerDeclaration,
                            symbol,
                            indexerDeclaration.Type,
                            indexerDeclaration.ParameterList,
                            indexerDeclaration.ExpressionBody,
                            generationContext ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                     && this.ShouldGenerateEmptyMember( symbol ) )
                {
                    members.Add(
                        this.GetEmptyImplIndexer(
                            symbol,
                            indexerDeclaration.Type,
                            indexerDeclaration.ParameterList,
                            indexerDeclaration.AccessorList.AssertNotNull() ) );
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

                return new[] { GetLinkedDeclaration( IntermediateSymbolSemanticKind.Default ) };
            }
            else
            {
                return new[] { GetLinkedDeclaration( IntermediateSymbolSemanticKind.Default ) };
            }

            MemberDeclarationSyntax GetLinkedDeclaration( IntermediateSymbolSemanticKind semanticKind )
            {
                var transformedAccessors = new List<AccessorDeclarationSyntax>();

                if ( symbol.GetMethod != null )
                {
                    if ( indexerDeclaration.AccessorList?.Accessors.SingleOrDefault( a => a.IsKind( SyntaxKind.GetAccessorDeclaration ) ) is
                        { } getAccessorDeclaration )
                    {
                        transformedAccessors.Add( GetLinkedAccessor( semanticKind, getAccessorDeclaration, symbol.GetMethod ) );
                    }
                    else if ( indexerDeclaration.ExpressionBody != null )
                    {
                        var linkedBody = this.GetSubstitutedBody(
                            symbol.GetMethod.ToSemantic( semanticKind ),
                            new SubstitutionContext(
                                this,
                                generationContext,
                                new InliningContextIdentifier( symbol.GetMethod.ToSemantic( semanticKind ) ) ) );

                        transformedAccessors.Add(
                            AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration,
                                    List<AttributeListSyntax>(),
                                    TokenList(),
                                    Block( linkedBody )
                                        .WithOpenBraceToken( Token( TriviaList( ElasticLineFeed ), SyntaxKind.OpenBraceToken, TriviaList( ElasticLineFeed ) ) )
                                        .WithCloseBraceToken(
                                            Token( TriviaList( ElasticLineFeed ), SyntaxKind.CloseBraceToken, TriviaList( ElasticMarker ) ) ) )
                                .WithKeyword( Token( TriviaList( ElasticMarker ), SyntaxKind.GetKeyword, TriviaList( ElasticMarker ) ) ) );
                    }
                }

                if ( symbol.SetMethod != null )
                {
                    var setAccessorDeclaration = indexerDeclaration.AccessorList!.Accessors.Single(
                        a => a.IsKind( SyntaxKind.SetAccessorDeclaration ) || a.IsKind( SyntaxKind.InitAccessorDeclaration ) );

                    transformedAccessors.Add( GetLinkedAccessor( semanticKind, setAccessorDeclaration, symbol.SetMethod ) );
                }

                // Trivia processing for the property declaration:
                //   * Properties with accessor lists are not changed (accessors are expected to be correct, accessor lists inherit the same trivia).
                //   * Properties with expression body are transformed in the following way (trivia of the <expr> are already processed):
                //      int Property <trivia_leading_equals_value> => <trivia_trailing_equals_value> <expr> <trivia_leading_semicolon>; <trivia_trailing_semicolon>
                //      int Property <trivia_leading_equals_value> { <trivia_after_equals_value> get { ... } <trivia_before_semicolon> } <trivia_trailing_semicolon>

                var (accessorListLeadingTrivia, accessorStartingTrivia, accessorEndingTrivia, accessorListTrailingTrivia) = indexerDeclaration switch
                {
                    { AccessorList: not null and var accessorList } => (
                        accessorList.OpenBraceToken.LeadingTrivia,
                        accessorList.OpenBraceToken.TrailingTrivia,
                        accessorList.CloseBraceToken.LeadingTrivia,
                        accessorList.CloseBraceToken.TrailingTrivia),
                    { ExpressionBody: not null and var expressionBody } => (
                        expressionBody.ArrowToken.LeadingTrivia,
                        expressionBody.ArrowToken.TrailingTrivia,
                        indexerDeclaration.SemicolonToken.LeadingTrivia,
                        indexerDeclaration.SemicolonToken.TrailingTrivia),
                    _ => throw new AssertionFailedException( $"Unexpected property declaration at '{indexerDeclaration.GetLocation()}'." )
                };

                accessorListLeadingTrivia =
                    indexerDeclaration.ThisKeyword switch
                    {
                        var thisKeyword when thisKeyword.TrailingTrivia.HasAnyNewLine() => accessorListLeadingTrivia,
                        _ => TriviaList( ElasticLineFeed ).AddRange( accessorListLeadingTrivia )
                    };

                return
                    indexerDeclaration
                        .WithAccessorList(
                            AccessorList(
                                    Token( accessorListLeadingTrivia, SyntaxKind.OpenBraceToken, accessorStartingTrivia ),
                                    List( transformedAccessors ),
                                    Token( accessorEndingTrivia, SyntaxKind.CloseBraceToken, accessorListTrailingTrivia ) )
                                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )
                        .WithExpressionBody( null )
                        .WithSemicolonToken( default );
            }

            AccessorDeclarationSyntax GetLinkedAccessor(
                IntermediateSymbolSemanticKind semanticKind,
                AccessorDeclarationSyntax accessorDeclaration,
                IMethodSymbol methodSymbol )
            {
                var linkedBody = this.GetSubstitutedBody(
                    methodSymbol.ToSemantic( semanticKind ),
                    new SubstitutionContext(
                        this,
                        generationContext,
                        new InliningContextIdentifier( methodSymbol.ToSemantic( semanticKind ) ) ) );

                // Trivia processing:
                //   * For block bodies methods, we preserve trivia of the opening/closing brace.
                //   * For expression bodied methods:
                //       int Foo() <trivia_leading_equals_value> => <trivia_trailing_equals_value> <expression> <trivia_leading_semicolon> ; <trivia_trailing_semicolon>
                //       int Foo() <trivia_leading_equals_value> { <trivia_trailing_equals_value> <linked_body> <trivia_leading_semicolon> } <trivia_trailing_semicolon>

                var (openBraceLeadingTrivia, openBraceTrailingTrivia, closeBraceLeadingTrivia, closeBraceTrailingTrivia) =
                    accessorDeclaration switch
                    {
                        { Body: { OpenBraceToken: var openBraceToken, CloseBraceToken: var closeBraceToken } } =>
                            (openBraceToken.LeadingTrivia, openBraceToken.TrailingTrivia, closeBraceToken.LeadingTrivia, closeBraceToken.TrailingTrivia),
                        { ExpressionBody.ArrowToken: var arrowToken, SemicolonToken: var semicolonToken } =>
                            (arrowToken.LeadingTrivia.Add( ElasticLineFeed ), arrowToken.TrailingTrivia.Add( ElasticLineFeed ),
                             semicolonToken.LeadingTrivia.Add( ElasticLineFeed ), semicolonToken.TrailingTrivia),
                        { SemicolonToken: var semicolonToken } => (
                            semicolonToken.LeadingTrivia.Add( ElasticLineFeed ), semicolonToken.TrailingTrivia.Add( ElasticLineFeed ),
                            TriviaList( ElasticLineFeed ), TriviaList( ElasticLineFeed )),
                        _ => throw new AssertionFailedException( $"Unexpected accessor declaration at '{accessorDeclaration.GetLocation()}'." )
                    };

                return accessorDeclaration
                    .WithExpressionBody( null )
                    .WithBody(
                        Block( linkedBody )
                            .WithOpenBraceToken( Token( openBraceLeadingTrivia, SyntaxKind.OpenBraceToken, openBraceTrailingTrivia ) )
                            .WithCloseBraceToken( Token( closeBraceLeadingTrivia, SyntaxKind.CloseBraceToken, closeBraceTrailingTrivia ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock )
                            .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )
                    .WithSemicolonToken( default );
            }
        }

        private static BlockSyntax GetImplicitIndexerGetterBody( IMethodSymbol symbol, SyntaxGenerationContext generationContext )
            => SyntaxFactoryEx.FormattedBlock(
                    ReturnStatement(
                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Space ),
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            symbol.IsStatic
                                ? generationContext.SyntaxGenerator.Type( symbol.ContainingType )
                                : ThisExpression(),
                            IdentifierName( GetBackingFieldName( (IPropertySymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                        Token( SyntaxKind.SemicolonToken ) ) )
                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );

        private static BlockSyntax GetImplicitIndexerSetterBody( IMethodSymbol symbol, SyntaxGenerationContext generationContext )
            => SyntaxFactoryEx.FormattedBlock(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                symbol.IsStatic
                                    ? generationContext.SyntaxGenerator.Type( symbol.ContainingType )
                                    : ThisExpression(),
                                IdentifierName( GetBackingFieldName( (IPropertySymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                            IdentifierName( "value" ) ) ) )
                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );

        private MemberDeclarationSyntax GetOriginalImplIndexer(
            IndexerDeclarationSyntax indexer,
            IPropertySymbol symbol,
            TypeSyntax type,
            BracketedParameterListSyntax parameterList,
            ArrowExpressionClauseSyntax? existingExpressionBody,
            SyntaxGenerationContext generationContext )
        {
            var existingAccessorList = indexer.AccessorList.AssertNotNull();

            var transformedAccessorList =
                existingAccessorList
                    .WithAccessors(
                        List(
                            existingAccessorList.Accessors.SelectAsArray(
                                a =>
                                    TransformAccessor(
                                        a,
                                        a.Kind() switch
                                        {
                                            SyntaxKind.GetAccessorDeclaration => symbol.GetMethod.AssertNotNull(),
                                            SyntaxKind.SetAccessorDeclaration or SyntaxKind.InitAccessorDeclaration => symbol.SetMethod.AssertNotNull(),
                                            _ => throw new AssertionFailedException( $"Unexpected kind:{a.Kind()}" )
                                        } ) ) ) )
                    .WithSourceCodeAnnotation();

            return this.GetSpecialImplIndexer(
                type,
                parameterList,
                transformedAccessorList,
                existingExpressionBody,
                symbol,
                GetOriginalImplParameterType() );

            AccessorDeclarationSyntax TransformAccessor( AccessorDeclarationSyntax accessorDeclaration, IMethodSymbol accessorSymbol )
            {
                var semantic = accessorSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default );
                var context = new InliningContextIdentifier( semantic );

                var substitutedBody =
                    accessorDeclaration.Body != null
                        ? (BlockSyntax) RewriteBody(
                            accessorDeclaration.Body,
                            accessorSymbol,
                            new SubstitutionContext( this, generationContext, context ) )
                        : null;

                var substitutedExpressionBody =
                    accessorDeclaration.ExpressionBody != null
                        ? (ArrowExpressionClauseSyntax) RewriteBody(
                            accessorDeclaration.ExpressionBody,
                            accessorSymbol,
                            new SubstitutionContext( this, generationContext, context ) )
                        : null;

                return
                    accessorDeclaration
                        .WithBody( substitutedBody )
                        .WithExpressionBody( substitutedExpressionBody );
            }
        }

        private MemberDeclarationSyntax GetEmptyImplIndexer(
            IPropertySymbol symbol,
            TypeSyntax type,
            BracketedParameterListSyntax parameterList,
            AccessorListSyntax existingAccessorList )
        {
            return this.GetSpecialImplIndexer( type, parameterList, existingAccessorList, null, symbol, GetEmptyImplParameterType() );
        }

        private MemberDeclarationSyntax GetSpecialImplIndexer(
            TypeSyntax indexerType,
            BracketedParameterListSyntax indexerParameters,
            AccessorListSyntax? accessorList,
            ArrowExpressionClauseSyntax? expressionBody,
            IPropertySymbol symbol,
            TypeSyntax specialImplType )
        {
            return
                IndexerDeclaration(
                        this.FilterAttributesOnSpecialImpl( symbol ),
                        symbol.IsStatic
                            ? TokenList(
                                Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ),
                                Token( SyntaxKind.StaticKeyword ).WithTrailingTrivia( Space ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ) ),
                        indexerType,
                        null,
                        Token( SyntaxKind.ThisKeyword ),
                        this.FilterAttributesOnSpecialImpl(
                            symbol.Parameters,
                            indexerParameters.WithAdditionalParameters( (specialImplType, AspectReferenceSyntaxProvider.LinkerOverrideParamName ) ) ),
                        null,
                        null,
                        default )
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithAccessorList( accessorList )
                    .WithExpressionBody( expressionBody )
                    .WithSemicolonToken( expressionBody != null ? Token( SyntaxKind.SemicolonToken ) : default )
                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );
        }

        private static IndexerDeclarationSyntax GetTrampolineForIndexer( IndexerDeclarationSyntax indexer, IPropertySymbol targetSymbol )
        {
            var getAccessor = indexer.AccessorList?.Accessors.SingleOrDefault( x => x.Kind() == SyntaxKind.GetAccessorDeclaration );
            var setAccessor = indexer.AccessorList?.Accessors.SingleOrDefault( x => x.Kind() == SyntaxKind.SetAccessorDeclaration );

            return indexer
                .WithAccessorList(
                    AccessorList(
                        List(
                            new[]
                                {
                                    getAccessor != null
                                        ? AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration,
                                                SyntaxFactoryEx.FormattedBlock(
                                                    ReturnStatement(
                                                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Space ),
                                                        GetInvocationTarget(),
                                                        Token( SyntaxKind.SemicolonToken ) ) ) )
                                        : null,
                                    setAccessor != null
                                        ? AccessorDeclaration(
                                                SyntaxKind.SetAccessorDeclaration,
                                                SyntaxFactoryEx.FormattedBlock(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.SimpleAssignmentExpression,
                                                            GetInvocationTarget(),
                                                            IdentifierName( "value" ) ) ) ) )
                                        : null
                                }.Where( a => a != null )
                                .AssertNoneNull() ) ) )
                .WithExpressionBody( null )
                .WithSemicolonToken( default )
                .WithLeadingTrivia( indexer.GetLeadingTrivia() )
                .WithTrailingTrivia( indexer.GetTrailingTrivia() );

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