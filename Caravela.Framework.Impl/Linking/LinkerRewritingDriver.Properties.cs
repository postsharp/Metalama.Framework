// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking.Inlining;
using Caravela.Framework.Impl.Pipeline;
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
        /// <param name="symbol">Override property symbol or overridden property symbol.</param>
        /// <returns></returns>
        private bool IsDiscarded( IPropertySymbol symbol, ResolvedAspectReferenceSemantic semantic )
        {
            if ( this._analysisRegistry.IsOverride( symbol ) )
            {
                var overrideTarget = this._analysisRegistry.GetOverrideTarget( symbol );
                var lastOverride = this._analysisRegistry.GetLastOverride( overrideTarget.AssertNotNull() );
                var getAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.PropertyGetAccessor );
                var setAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.PropertySetAccessor );

                if ( SymbolEqualityComparer.Default.Equals( symbol, lastOverride ) )
                {
                    return this.IsInlineable( symbol, semantic );
                }
                else
                {
                    return this.IsInlineable( symbol, semantic ) || (getAspectReferences.Count == 0 && setAspectReferences.Count == 0);
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

            return (getAspectReferences.Count == 0 || this.IsInlineableReference( getAspectReferences[0] ))
                   && (setAspectReferences.Count == 0 || this.IsInlineableReference( setAspectReferences[0] ));
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

                if ( IsAutoPropertyDeclaration( propertyDeclaration ) )
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
                if ( this.IsDiscarded( symbol, ResolvedAspectReferenceSemantic.Default ) )
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
        {
            return
                FieldDeclaration(
                        List<AttributeListSyntax>(),
                        symbol.IsStatic
                            ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                        VariableDeclaration(
                            propertyDeclaration.Type,
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier( GetAutoPropertyBackingFieldName( symbol ) ),
                                    null,
                                    propertyDeclaration.Initializer ) ) ) )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( LineFeed )
                    .WithTrailingTrivia( LineFeed, LineFeed )
                    .WithAdditionalAnnotations( AspectPipelineAnnotations.GeneratedCode );
        }

        private static BlockSyntax GetImplicitGetterBody( IMethodSymbol symbol )
        {
            return Block(
                ReturnStatement(
                    Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        symbol.IsStatic
                            ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( symbol.ContainingType )
                            : ThisExpression(),
                        IdentifierName( GetAutoPropertyBackingFieldName( (IPropertySymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                    Token( SyntaxKind.SemicolonToken ) ) );
        }

        private static BlockSyntax GetImplicitSetterBody( IMethodSymbol symbol )
        {
            return Block(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            symbol.IsStatic
                                ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( symbol.ContainingType )
                                : ThisExpression(),
                            IdentifierName( GetAutoPropertyBackingFieldName( (IPropertySymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                        IdentifierName( "value" ) ) ) );
        }

        private static string GetAutoPropertyBackingFieldName( IPropertySymbol property )
        {
            var firstPropertyLetter = property.Name.Substring( 0, 1 );
            var camelCasePropertyName = firstPropertyLetter.ToLowerInvariant() + (property.Name.Length > 1 ? property.Name.Substring( 1 ) : "");

            if ( property.ContainingType.GetMembers( camelCasePropertyName ).Any() && firstPropertyLetter == firstPropertyLetter.ToLowerInvariant() )
            {
                // If there there is another property whose name differs only by the case of the first character, then the lower case variant will be suffixed.
                // This is unlikely given naming standards.

                camelCasePropertyName = FindUniqueName( camelCasePropertyName );
            }

            // TODO: Write tests of the collision resolution algorithm.
            if ( camelCasePropertyName.StartsWith( "_", StringComparison.Ordinal ) )
            {
                return camelCasePropertyName;
            }
            else
            {
                var fieldName = FindUniqueName( "_" + camelCasePropertyName );

                return fieldName;
            }

            string FindUniqueName( string hint )
            {
                if ( !property.ContainingType.GetMembers( hint ).Any() )
                {
                    return hint;
                }
                else
                {
                    for ( var i = 1; /* Nothing */; i++ )
                    {
                        var candidate = hint + i;

                        if ( !property.ContainingType.GetMembers( candidate ).Any() )
                        {
                            return candidate;
                        }
                    }
                }
            }
        }

        private static bool IsAutoPropertyDeclaration( PropertyDeclarationSyntax propertyDeclaration )
        {
            return propertyDeclaration.ExpressionBody == null
                   && propertyDeclaration.AccessorList?.Accessors.All( x => x.Body == null && x.ExpressionBody == null ) == true
                   && propertyDeclaration.Modifiers.All( x => x.Kind() != SyntaxKind.AbstractKeyword );
        }

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

            var initializer =
                property.Initializer?.AddSourceCodeAnnotation();

            return
                PropertyDeclaration(
                        List<AttributeListSyntax>(),
                        symbol.IsStatic
                            ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                        property.Type,
                        null,
                        Identifier( GetOriginalImplMemberName( property.Identifier.ValueText ) ),
                        null,
                        null,
                        null )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithInitializer( initializer )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithAccessorList( accessorList );
        }
    }
}