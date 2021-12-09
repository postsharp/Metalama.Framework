// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Inlining;
using Metalama.Framework.Engine.Utilities;
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
        private IReadOnlyList<MemberDeclarationSyntax> RewriteProperty( PropertyDeclarationSyntax propertyDeclaration, IPropertySymbol symbol )
        {
            var generationContext = SyntaxGenerationContext.Create( this._serviceProvider, this._intermediateCompilation, propertyDeclaration );

            if ( this._introductionRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IPropertySymbol) this._introductionRegistry.GetLastOverride( symbol );

                if ( propertyDeclaration.IsAutoPropertyDeclaration()
                     && this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ) ) )
                {
                    // Backing field for auto property.
                    members.Add( GetPropertyBackingField( propertyDeclaration, symbol ) );
                }

                if ( this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( lastOverride, IntermediateSymbolSemanticKind.Default ), out _ ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final ) );
                }
                else
                {
                    members.Add( GetTrampolineProperty( propertyDeclaration, lastOverride ) );
                }

                if ( this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ) )
                     && !this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ), out _ ) )
                {
                    members.Add( GetOriginalImplProperty( propertyDeclaration, symbol, generationContext ) );
                }

                if ( this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Base ) )
                     && !this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Base ), out _ ) )
                {
                    members.Add( GetEmptyImplProperty( propertyDeclaration, symbol ) );
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
                    switch ( symbol.GetMethod.GetPrimaryDeclaration().AssertNotNull() )
                    {
                        case AccessorDeclarationSyntax getAccessorDeclaration:
                            transformedAccessors.Add(
                                AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration,
                                    getAccessorDeclaration.AttributeLists,
                                    getAccessorDeclaration.Modifiers,
                                    this.GetLinkedBody(
                                        symbol.GetMethod.ToSemantic( semanticKind ),
                                        InliningContext.Create( this, symbol.GetMethod, generationContext ) ) ) );

                            break;

                        case ArrowExpressionClauseSyntax:
                            transformedAccessors.Add(
                                AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration,
                                    List<AttributeListSyntax>(),
                                    TokenList(),
                                    this.GetLinkedBody(
                                        symbol.GetMethod.ToSemantic( semanticKind ),
                                        InliningContext.Create( this, symbol.GetMethod, generationContext ) ) ) );

                            break;
                    }
                }

                if ( symbol.SetMethod != null )
                {
                    var setDeclaration = (AccessorDeclarationSyntax) symbol.SetMethod.GetPrimaryDeclaration().AssertNotNull();

                    transformedAccessors.Add(
                        AccessorDeclaration(
                            SyntaxKind.SetAccessorDeclaration,
                            setDeclaration.AttributeLists,
                            setDeclaration.Modifiers,
                            this.GetLinkedBody(
                                symbol.SetMethod.ToSemantic( semanticKind ),
                                InliningContext.Create( this, symbol.SetMethod, generationContext ) ) ) );
                }

                return
                    propertyDeclaration
                        .WithAccessorList( AccessorList( List( transformedAccessors ) ) )
                        .WithLeadingTrivia( propertyDeclaration.GetLeadingTrivia() )
                        .WithTrailingTrivia( propertyDeclaration.GetTrailingTrivia() )
                        .WithExpressionBody( null )
                        .WithInitializer( null )
                        .WithSemicolonToken( Token( SyntaxKind.None ) );
            }
        }

        private static FieldDeclarationSyntax GetPropertyBackingField( PropertyDeclarationSyntax propertyDeclaration, IPropertySymbol symbol )
            => FieldDeclaration(
                    List<AttributeListSyntax>(),
                    symbol.IsStatic
                        ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                        : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
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
                .AddGeneratedCodeAnnotation();

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
                .AddGeneratedCodeAnnotation();

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
                .AddGeneratedCodeAnnotation();

        private static MemberDeclarationSyntax GetOriginalImplProperty(
            PropertyDeclarationSyntax property,
            IPropertySymbol symbol,
            SyntaxGenerationContext generationContext )
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
                                                GetImplicitGetterBody( symbol.GetMethod, generationContext ) )
                                            : null,
                                        symbol.SetMethod != null
                                            ? AccessorDeclaration(
                                                SyntaxKind.SetAccessorDeclaration,
                                                GetImplicitSetterBody( symbol.SetMethod, generationContext ) )
                                            : null
                                    }.Where( a => a != null )
                                    .AssertNoneNull() ) )
                        .NormalizeWhitespace()
                    : property.AccessorList.AssertNotNull().AddSourceCodeAnnotation();

            var initializer = property.Initializer;

            return GetSpecialImplProperty( property.Type, accessorList, initializer.AddSourceCodeAnnotation(), symbol, GetOriginalImplMemberName( symbol ) );
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
                                                ArrowExpressionClause( DefaultExpression( property.Type ) ) )
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

            return GetSpecialImplProperty( property.Type, accessorList, null, symbol, GetEmptyImplMemberName( symbol ) );
        }

        private static MemberDeclarationSyntax GetSpecialImplProperty(
            TypeSyntax propertyType,
            AccessorListSyntax accessorList,
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
                    .WithInitializer( initializer.AddSourceCodeAnnotation() )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithAccessorList( accessorList )
                    .AddGeneratedCodeAnnotation();
        }
    }
}