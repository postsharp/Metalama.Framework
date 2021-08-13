// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Linking.Inlining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerRewritingDriver
    {
        /// <summary>
        /// Determines whether the property will be discarded in the final compilation (unreferenced or inlined declarations).
        /// </summary>
        /// <param name="referencedProperty">Override property symbol or overridden property symbol.</param>
        /// <returns></returns>
        private bool IsDiscarded( IPropertySymbol referencedProperty, ResolvedAspectReferenceSemantic semantic )
        {
            if ( this._analysisRegistry.IsOverride( referencedProperty ) )
            {
                var overrideTarget = this._analysisRegistry.GetOverrideTarget( referencedProperty );
                var lastOverride = this._analysisRegistry.GetLastOverride( overrideTarget.AssertNotNull() );

                var getAspectReferences = this._analysisRegistry.GetAspectReferences(
                    referencedProperty,
                    semantic,
                    AspectReferenceTargetKind.PropertyGetAccessor );

                var setAspectReferences = this._analysisRegistry.GetAspectReferences(
                    referencedProperty,
                    semantic,
                    AspectReferenceTargetKind.PropertySetAccessor );

                if ( SymbolEqualityComparer.Default.Equals( referencedProperty, lastOverride ) )
                {
                    return this.IsInlineable( referencedProperty, semantic );
                }
                else
                {
                    return this.IsInlineable( referencedProperty, semantic ) || (getAspectReferences.Count == 0 && setAspectReferences.Count == 0);
                }
            }
            else
            {
                return false;
            }
        }

        private bool IsInlineable( IPropertySymbol symbol, ResolvedAspectReferenceSemantic semantic )
        {
            if ( GetDeclarationFlags( symbol ).HasFlag( LinkerDeclarationFlags.NotInlineable ) )
            {
                return false;
            }

            if ( this._analysisRegistry.IsLastOverride( symbol ) )
            {
                return true;
            }

            var selfAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic );
            var getAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.PropertyGetAccessor );
            var setAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.PropertySetAccessor );

            if ( selfAspectReferences.Count > 0 )
            {
                // TODO: We may need to deal with this case.
                return false;
            }

            if ( getAspectReferences.Count > 1 || setAspectReferences.Count > 1
                                               || (getAspectReferences.Count == 0 && setAspectReferences.Count == 0) )
            {
                return false;
            }

            return (getAspectReferences.Count == 0 || this.IsInlineableReference( getAspectReferences[0], MethodKind.PropertyGet ))
                   && (setAspectReferences.Count == 0 || this.IsInlineableReference( setAspectReferences[0], MethodKind.PropertySet ));
        }

        private bool HasAnyAspectReferences( IPropertySymbol symbol, ResolvedAspectReferenceSemantic semantic )
        {
            var selfAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic );
            var getAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.PropertyGetAccessor );
            var setAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.PropertySetAccessor );

            return selfAspectReferences.Count > 0 || getAspectReferences.Count > 0 || setAspectReferences.Count > 0;
        }

        private IReadOnlyList<MemberDeclarationSyntax> RewriteProperty( PropertyDeclarationSyntax propertyDeclaration, IPropertySymbol symbol )
        {
            if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IPropertySymbol) this._analysisRegistry.GetLastOverride( symbol );

                if ( IsAutoPropertyDeclaration( propertyDeclaration )
                     && this.HasAnyAspectReferences( symbol, ResolvedAspectReferenceSemantic.Original ) )
                {
                    // Backing field for auto property.
                    members.Add( GetPropertyBackingField( propertyDeclaration, symbol ) );
                }

                if ( this.IsInlineable( lastOverride, ResolvedAspectReferenceSemantic.Default ) )
                {
                    members.Add( GetLinkedDeclaration() );
                }
                else
                {
                    members.Add( GetTrampolineProperty( propertyDeclaration, lastOverride ) );
                }

                if ( !this.IsInlineable( symbol, ResolvedAspectReferenceSemantic.Original )
                     && this.HasAnyAspectReferences( symbol, ResolvedAspectReferenceSemantic.Original ) )
                {
                    members.Add( GetOriginalImplProperty( propertyDeclaration, symbol ) );
                }

                return members;
            }
            else if ( this._analysisRegistry.IsOverride( symbol ) )
            {
                if ( this.IsDiscarded( (ISymbol) symbol, ResolvedAspectReferenceSemantic.Default ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[] { GetLinkedDeclaration() };
            }
            else
            {
                throw new AssertionFailedException();
            }

            MemberDeclarationSyntax GetLinkedDeclaration()
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
                                        this.GetBodySource( symbol.GetMethod ),
                                        InliningContext.Create( this, symbol.GetMethod ) ) ) );

                            break;

                        case ArrowExpressionClauseSyntax:
                            transformedAccessors.Add(
                                AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration,
                                    List<AttributeListSyntax>(),
                                    TokenList(),
                                    this.GetLinkedBody(
                                        this.GetBodySource( symbol.GetMethod ),
                                        InliningContext.Create( this, symbol.GetMethod ) ) ) );

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
                                this.GetBodySource( symbol.SetMethod ),
                                InliningContext.Create( this, symbol.SetMethod ) ) ) );
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

        private static BlockSyntax GetImplicitGetterBody( IMethodSymbol symbol )
            => Block(
                    ReturnStatement(
                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            symbol.IsStatic
                                ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( symbol.ContainingType )
                                : ThisExpression(),
                            IdentifierName( GetBackingFieldName( (IPropertySymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                        Token( SyntaxKind.SemicolonToken ) ) )
                .AddGeneratedCodeAnnotation();

        private static BlockSyntax GetImplicitSetterBody( IMethodSymbol symbol )
            => Block(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                symbol.IsStatic
                                    ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( symbol.ContainingType )
                                    : ThisExpression(),
                                IdentifierName( GetBackingFieldName( (IPropertySymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                            IdentifierName( "value" ) ) ) )
                .AddGeneratedCodeAnnotation();

        private static bool IsAutoPropertyDeclaration( PropertyDeclarationSyntax propertyDeclaration )
            => propertyDeclaration.ExpressionBody == null
               && propertyDeclaration.AccessorList?.Accessors.All( x => x.Body == null && x.ExpressionBody == null ) == true
               && propertyDeclaration.Modifiers.All( x => x.Kind() != SyntaxKind.AbstractKeyword );

        private static MemberDeclarationSyntax GetOriginalImplProperty( PropertyDeclarationSyntax property, IPropertySymbol symbol )
        {
            var accessorList =
                IsAutoPropertyDeclaration( property )
                    ? AccessorList(
                            List(
                                new[]
                                    {
                                        symbol.GetMethod != null
                                            ? AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration,
                                                GetImplicitGetterBody( symbol.GetMethod ) )
                                            : null,
                                        symbol.SetMethod != null
                                            ? AccessorDeclaration(
                                                SyntaxKind.SetAccessorDeclaration,
                                                GetImplicitSetterBody( symbol.SetMethod ) )
                                            : null
                                    }.Where( a => a != null )
                                    .AssertNoneNull() ) )
                        .NormalizeWhitespace()
                    : property.AccessorList.AddSourceCodeAnnotation();

            var initializer = property.Initializer;

            return
                PropertyDeclaration(
                        List<AttributeListSyntax>(),
                        symbol.IsStatic
                            ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                        property.Type,
                        null,
                        Identifier( GetOriginalImplMemberName( symbol ) ),
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