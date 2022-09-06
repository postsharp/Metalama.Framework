// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Inlining;
using Metalama.Framework.Engine.Linking.Substitution;
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
    internal partial class LinkerRewritingDriver
    {
        private IReadOnlyList<MemberDeclarationSyntax> RewriteProperty(
            PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol symbol,
            SyntaxGenerationContext generationContext )
        {
            if ( this.IntroductionRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IPropertySymbol) this.IntroductionRegistry.GetLastOverride( symbol );

                if ( propertyDeclaration.IsAutoPropertyDeclaration()
                     && this.AnalysisRegistry.IsReachable( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) )
                     && this.AnalysisRegistry.IsInlined( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) ) )
                {
                    // Backing field for auto property.
                    members.Add( GetPropertyBackingField( propertyDeclaration, symbol ) );
                }

                if ( this.AnalysisRegistry.IsInlined( lastOverride.ToSemantic(IntermediateSymbolSemanticKind.Default) ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final ) );
                }
                else
                {
                    members.Add( GetTrampolineProperty( propertyDeclaration, lastOverride ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) ) )
                {
                    members.Add( GetOriginalImplProperty( propertyDeclaration, symbol, generationContext, propertyDeclaration.IsAutoPropertyDeclaration() ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic(IntermediateSymbolSemanticKind.Base) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic(IntermediateSymbolSemanticKind.Base) ) )
                {
                    members.Add( GetEmptyImplProperty( propertyDeclaration, symbol ) );
                }

                return members;
            }
            else if ( this.IntroductionRegistry.IsOverride( symbol ) )
            {
                if ( !this.AnalysisRegistry.IsReachable( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) )
                     || this.AnalysisRegistry.IsInlined( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[] { GetLinkedDeclaration( IntermediateSymbolSemanticKind.Default ) };
            }
            else
            {
                throw new AssertionFailedException();
            }

            MemberDeclarationSyntax GetLinkedDeclaration( IntermediateSymbolSemanticKind semanticKind )
            {
                var transformedAccessors = new List<AccessorDeclarationSyntax>();

                if ( symbol.GetMethod != null )
                {
                    if ( propertyDeclaration.AccessorList != null
                         && propertyDeclaration.AccessorList.Accessors.SingleOrDefault( a => a.IsKind( SyntaxKind.GetAccessorDeclaration ) ) is
                             { } getAccessorDeclaration )
                    {
                        transformedAccessors.Add( GetLinkedAccessor( semanticKind, getAccessorDeclaration, symbol.GetMethod ) );
                    }
                    else if ( propertyDeclaration.ExpressionBody != null )
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
                    var setAccessorDeclaration = propertyDeclaration.AccessorList!.Accessors.Single(
                        a => a.IsKind( SyntaxKind.SetAccessorDeclaration ) || a.IsKind( SyntaxKind.InitAccessorDeclaration ) );

                    transformedAccessors.Add( GetLinkedAccessor( semanticKind, setAccessorDeclaration, symbol.SetMethod ) );
                }

                // Trivia processing for the property declaration:
                //   * Properties with accessor lists are not changed (accessors are expected to be correct, accessor lists inherit the same trivia).
                //   * Properties with expression body are transformed in the following way (trivia of the <expr> are already processed):
                //      int Property <trivia_leading_equals_value> => <trivia_trailing_equals_value> <expr> <trivia_leading_semicolon>; <trivia_trailing_semicolon>
                //      int Property <trivia_leading_equals_value> { <trivia_after_equals_value> get { ... } <trivia_before_semicolon> } <trivia_trailing_semicolon>

                var (accessorListLeadingTrivia, accessorStartingTrivia, accessorEndingTrivia, accessorListTrailingTrivia) = propertyDeclaration switch
                {
                    { AccessorList: not null and var accessorList } => (
                        accessorList.OpenBraceToken.LeadingTrivia,
                        accessorList.OpenBraceToken.TrailingTrivia,
                        accessorList.CloseBraceToken.LeadingTrivia,
                        accessorList.CloseBraceToken.TrailingTrivia),
                    { ExpressionBody: not null and var expressionBody } => (
                        expressionBody.ArrowToken.LeadingTrivia,
                        expressionBody.ArrowToken.TrailingTrivia,
                        propertyDeclaration.SemicolonToken.LeadingTrivia,
                        propertyDeclaration.SemicolonToken.TrailingTrivia),
                    _ => throw new AssertionFailedException()
                };

                accessorListLeadingTrivia =
                    propertyDeclaration.Identifier switch
                    {
                        var identifier when identifier.TrailingTrivia.HasAnyNewLine() => accessorListLeadingTrivia,
                        _ => TriviaList( ElasticLineFeed ).AddRange( accessorListLeadingTrivia )
                    };

                return
                    propertyDeclaration
                        .WithAccessorList(
                            AccessorList(
                                    Token( accessorListLeadingTrivia, SyntaxKind.OpenBraceToken, accessorStartingTrivia ),
                                    List( transformedAccessors ),
                                    Token( accessorEndingTrivia, SyntaxKind.CloseBraceToken, accessorListTrailingTrivia ) )
                                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )
                        .WithExpressionBody( null )
                        .WithInitializer( null )
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
                        { ExpressionBody: { ArrowToken: var arrowToken }, SemicolonToken: var semicolonToken } =>
                            (arrowToken.LeadingTrivia.Add( ElasticLineFeed ), arrowToken.TrailingTrivia.Add( ElasticLineFeed ),
                             semicolonToken.LeadingTrivia.Add( ElasticLineFeed ), semicolonToken.TrailingTrivia),
                        { SemicolonToken: var semicolonToken } => (
                            semicolonToken.LeadingTrivia.Add( ElasticLineFeed ), semicolonToken.TrailingTrivia.Add( ElasticLineFeed ),
                            TriviaList( ElasticLineFeed ), TriviaList( ElasticLineFeed )),
                        _ => throw new AssertionFailedException()
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

        private static FieldDeclarationSyntax GetPropertyBackingField( PropertyDeclarationSyntax propertyDeclaration, IPropertySymbol symbol )
        {
            var modifiers = new List<SyntaxToken> { Token( SyntaxKind.PrivateKeyword ) };

            if ( symbol.SetMethod == null || symbol.SetMethod.IsInitOnly )
            {
                modifiers.Add( Token( SyntaxKind.ReadOnlyKeyword ) );
            }

            if ( symbol.IsStatic )
            {
                modifiers.Add( Token( SyntaxKind.StaticKeyword ) );
            }

            return FieldDeclaration(
                    List<AttributeListSyntax>(),
                    TokenList( modifiers ),
                    VariableDeclaration(
                        propertyDeclaration.Type,
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier( GetBackingFieldName( symbol ) ),
                                null,
                                propertyDeclaration.Initializer ) ) ) )
                .NormalizeWhitespace()
                .WithLeadingTrivia( LineFeed, LineFeed )
                .WithTrailingTrivia( LineFeed )
                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );
        }

        private static BlockSyntax GetImplicitGetterBody( IMethodSymbol symbol, SyntaxGenerationContext generationContext )
            => Block(
                    ReturnStatement(
                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            symbol.IsStatic
                                ? generationContext.SyntaxGenerator.Type( symbol.ContainingType )
                                : ThisExpression(),
                            IdentifierName( GetBackingFieldName( (IPropertySymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                        Token( SyntaxKind.SemicolonToken ) ) )
                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );

        private static BlockSyntax GetImplicitSetterBody( IMethodSymbol symbol, SyntaxGenerationContext generationContext )
            => Block(
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

        private static MemberDeclarationSyntax GetOriginalImplProperty(
            PropertyDeclarationSyntax property,
            IPropertySymbol symbol,
            SyntaxGenerationContext generationContext,
            bool autoProperty )
        {
            var accessorList =
                property.IsAutoPropertyDeclaration()
                    ? AccessorList(
                            List(
                                new[]
                                    {
                                        symbol.GetMethod != null
                                            ? autoProperty
                                                ? AccessorDeclaration(
                                                    SyntaxKind.GetAccessorDeclaration,
                                                    List<AttributeListSyntax>(),
                                                    TokenList(),
                                                    Token( SyntaxKind.GetKeyword ),
                                                    null,
                                                    null,
                                                    Token( SyntaxKind.SemicolonToken ) )
                                                : AccessorDeclaration(
                                                    SyntaxKind.GetAccessorDeclaration,
                                                    GetImplicitGetterBody( symbol.GetMethod, generationContext ) )
                                            : null,
                                        symbol.SetMethod != null
                                            ? autoProperty
                                                ? AccessorDeclaration(
                                                    SyntaxKind.SetAccessorDeclaration,
                                                    List<AttributeListSyntax>(),
                                                    TokenList(),
                                                    Token( SyntaxKind.SetKeyword ),
                                                    null,
                                                    null,
                                                    Token( SyntaxKind.SemicolonToken ) )
                                                : AccessorDeclaration(
                                                    SyntaxKind.SetAccessorDeclaration,
                                                    GetImplicitSetterBody( symbol.SetMethod, generationContext ) )
                                            : null
                                    }.Where( a => a != null )
                                    .AssertNoneNull() ) )
                        .NormalizeWhitespace()
                    : property.AccessorList?.WithSourceCodeAnnotation();

            var expressionBody =
                property.IsAutoPropertyDeclaration()
                    ? null
                    : property.AccessorList != null
                        ? null
                        : property.ExpressionBody;

            var initializer = property.Initializer;

            return GetSpecialImplProperty(
                property.Type,
                accessorList,
                expressionBody,
                initializer.WithSourceCodeAnnotation(),
                symbol,
                GetOriginalImplMemberName( symbol ) );
        }

        private static MemberDeclarationSyntax GetEmptyImplProperty( PropertyDeclarationSyntax property, IPropertySymbol symbol )
        {
            var accessorList =
                property.IsAutoPropertyDeclaration()
                    ? AccessorList(
                            List(
                                new[]
                                    {
                                        symbol.GetMethod != null
                                            ? AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration,
                                                List<AttributeListSyntax>(),
                                                TokenList(),
                                                Token( SyntaxKind.GetKeyword ),
                                                null,
                                                ArrowExpressionClause( DefaultExpression( property.Type ) ),
                                                Token( SyntaxKind.SemicolonToken ) )
                                            : null,
                                        symbol.SetMethod != null
                                            ? AccessorDeclaration(
                                                SyntaxKind.SetAccessorDeclaration,
                                                Block() )
                                            : null
                                    }.Where( a => a != null )
                                    .AssertNoneNull() ) )
                        .NormalizeWhitespace()
                    : property.AccessorList.AssertNotNull();

            return GetSpecialImplProperty( property.Type, accessorList, null, null, symbol, GetEmptyImplMemberName( symbol ) );
        }

        private static MemberDeclarationSyntax GetSpecialImplProperty(
            TypeSyntax propertyType,
            AccessorListSyntax? accessorList,
            ArrowExpressionClauseSyntax? expressionBody,
            EqualsValueClauseSyntax? initializer,
            IPropertySymbol symbol,
            string name )
        {
            return
                PropertyDeclaration(
                        List<AttributeListSyntax>(),
                        symbol.IsStatic
                            ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                        propertyType,
                        null,
                        Identifier( name ),
                        null,
                        null,
                        null )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithAccessorList( accessorList )
                    .WithExpressionBody( expressionBody )
                    .WithInitializer( initializer.WithSourceCodeAnnotation() )
                    .WithSemicolonToken( expressionBody != null || initializer != null ? Token( SyntaxKind.SemicolonToken ) : default )
                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );
        }
    }
}