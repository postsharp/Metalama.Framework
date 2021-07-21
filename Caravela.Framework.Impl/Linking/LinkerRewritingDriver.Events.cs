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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerRewritingDriver
    {
        /// <summary>
        /// Determines whether the event will be discarded in the final compilation (unreferenced or inlined declarations).
        /// </summary>
        /// <param name="symbol">Override event symbol or overridden event symbol.</param>
        /// <returns></returns>
        private bool IsDiscarded( IEventSymbol symbol, ResolvedAspectReferenceSemantic semantic )
        {
            var addAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.EventAddAccessor );
            var removeAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.EventRemoveAccessor );

            if ( addAspectReferences.Count == 0 && removeAspectReferences.Count == 0 )
            {
                return true;
            }

            if ( this.IsInlineable( symbol, semantic ) )
            {
                return true;
            }

            return false;
        }

        private bool IsInlineable( IEventSymbol symbol, ResolvedAspectReferenceSemantic semantic )
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
            var addAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.EventAddAccessor );
            var removeAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.EventRemoveAccessor );

            if ( selfAspectReferences.Count > 0 )
            {
                // TODO: We may need to deal with this case.
                return false;
            }

            if ( addAspectReferences.Count > 1 || removeAspectReferences.Count > 1 )
            {
                return false;
            }

            if ( addAspectReferences.Count == 0 && removeAspectReferences.Count == 0 )
            {
                return false;
            }

            return (addAspectReferences.Count == 0 || this.IsInlineableReference( addAspectReferences[0] ))
                   && (removeAspectReferences.Count == 0 || this.IsInlineableReference( removeAspectReferences[0] ));
        }

        private bool HasAnyAspectReferences( IEventSymbol symbol, ResolvedAspectReferenceSemantic semantic )
        {
            var selfAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic );
            var addAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.EventAddAccessor );
            var removeAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic, AspectReferenceTargetKind.EventRemoveAccessor );

            return selfAspectReferences.Count > 0 || addAspectReferences.Count > 0 || removeAspectReferences.Count > 0;
        }

        private IReadOnlyList<MemberDeclarationSyntax> RewriteEvent( EventDeclarationSyntax eventDeclaration, IEventSymbol symbol )
        {
            if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax> { GetLinkedDeclaration() };

                if ( !this.IsInlineable( (IEventSymbol) this._analysisRegistry.GetLastOverride( symbol ), ResolvedAspectReferenceSemantic.Default ) )
                {
                    members.Add( GetTrampolineEvent( eventDeclaration, symbol ) );
                }

                if ( !this.IsInlineable( symbol, ResolvedAspectReferenceSemantic.Original )
                     && this.HasAnyAspectReferences( symbol, ResolvedAspectReferenceSemantic.Original ) )
                {
                    members.Add( GetOriginalImplEvent( eventDeclaration, symbol ) );
                }

                return members;
            }
            else
            {
                if ( this.IsDiscarded( symbol, ResolvedAspectReferenceSemantic.Default ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[] { GetLinkedDeclaration() };
            }

            MemberDeclarationSyntax GetLinkedDeclaration()
            {
                var addDeclaration = (AccessorDeclarationSyntax) symbol.AddMethod.AssertNotNull().GetPrimaryDeclaration().AssertNotNull();

                var transformedAdd =
                    AccessorDeclaration(
                        SyntaxKind.AddAccessorDeclaration,
                        addDeclaration.AttributeLists,
                        TokenList(),
                        this.GetLinkedBody(
                            this.GetBodySource( symbol.AddMethod.AssertNotNull() ),
                            InliningContext.Create( this, symbol.AddMethod.AssertNotNull() ) ) );

                var removeDeclaration = (AccessorDeclarationSyntax) symbol.RemoveMethod.AssertNotNull().GetPrimaryDeclaration().AssertNotNull();

                var transformedRemove =
                    AccessorDeclaration(
                        SyntaxKind.RemoveAccessorDeclaration,
                        removeDeclaration.AttributeLists,
                        TokenList(),
                        this.GetLinkedBody(
                            this.GetBodySource( symbol.RemoveMethod.AssertNotNull() ),
                            InliningContext.Create( this, symbol.RemoveMethod.AssertNotNull() ) ) );

                return eventDeclaration
                    .WithAccessorList( AccessorList( List( new[] { transformedAdd, transformedRemove } ) ) )
                    .WithLeadingTrivia( eventDeclaration.GetLeadingTrivia() )
                    .WithTrailingTrivia( eventDeclaration.GetTrailingTrivia() );
            }
        }

        private IReadOnlyList<MemberDeclarationSyntax> RewriteEventField( EventFieldDeclarationSyntax eventFieldDeclaration, IEventSymbol symbol )
        {
            if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax> { GetEventBackingField( eventFieldDeclaration, symbol ), GetLinkedDeclaration() };

                if ( !this.IsInlineable( (IEventSymbol) this._analysisRegistry.GetLastOverride( symbol ), ResolvedAspectReferenceSemantic.Default ) )
                {
                    members.Add( GetTrampolineEvent( eventFieldDeclaration, symbol ) );
                }

                if ( !this.IsInlineable( symbol, ResolvedAspectReferenceSemantic.Original )
                     && this.HasAnyAspectReferences( symbol, ResolvedAspectReferenceSemantic.Original ) )
                {
                    members.Add( GetOriginalImplEvent( eventFieldDeclaration, symbol ) );
                }

                return members;
            }
            else
            {
                if ( this.IsDiscarded( symbol, ResolvedAspectReferenceSemantic.Default ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[] { GetLinkedDeclaration() };
            }

            MemberDeclarationSyntax GetLinkedDeclaration()
            {
                var transformedAdd =
                    AccessorDeclaration(
                        SyntaxKind.AddAccessorDeclaration,
                        List<AttributeListSyntax>(),
                        TokenList(),
                        this.GetLinkedBody(
                            this.GetBodySource( symbol.AddMethod.AssertNotNull() ),
                            InliningContext.Create( this, symbol.AddMethod.AssertNotNull() ) ) );

                var transformedRemove =
                    AccessorDeclaration(
                        SyntaxKind.RemoveAccessorDeclaration,
                        List<AttributeListSyntax>(),
                        TokenList(),
                        this.GetLinkedBody(
                            this.GetBodySource( symbol.RemoveMethod.AssertNotNull() ),
                            InliningContext.Create( this, symbol.RemoveMethod.AssertNotNull() ) ) );

                return
                    EventDeclaration(
                            List<AttributeListSyntax>(),
                            eventFieldDeclaration.Modifiers,
                            Token( SyntaxKind.EventKeyword ).WithTrailingTrivia( ElasticSpace ),
                            eventFieldDeclaration.Declaration.Type,
                            null,
                            Identifier( symbol.Name ),
                            AccessorList( List( new[] { transformedAdd, transformedRemove } ) ),
                            MissingToken( SyntaxKind.SemicolonToken ) )
                        .WithLeadingTrivia( eventFieldDeclaration.GetLeadingTrivia() )
                        .WithTrailingTrivia( eventFieldDeclaration.GetTrailingTrivia() );
            }
        }

        private static BlockSyntax GetImplicitAdderBody( IMethodSymbol symbol )
        {
            return Block(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.AddAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            symbol.IsStatic
                                ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( symbol.ContainingType )
                                : ThisExpression(),
                            IdentifierName( GetEventFieldBackingFieldName( (IEventSymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                        IdentifierName( "value" ) ) ) );
        }

        private static BlockSyntax GetImplicitRemoverBody( IMethodSymbol symbol )
        {
            return Block(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SubtractAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            symbol.IsStatic
                                ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( symbol.ContainingType )
                                : ThisExpression(),
                            IdentifierName( GetEventFieldBackingFieldName( (IEventSymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                        IdentifierName( "value" ) ) ) );
        }

        public static string GetEventFieldBackingFieldName( IEventSymbol @event )
        {
            return $"__{@event.Name}__BackingField";
        }

        private static FieldDeclarationSyntax GetEventBackingField( EventFieldDeclarationSyntax eventFieldDeclaration, IEventSymbol symbol )
        {
            return
                FieldDeclaration(
                        List<AttributeListSyntax>(),
                        symbol.IsStatic
                            ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                        VariableDeclaration(
                            eventFieldDeclaration.Declaration.Type,
                            SingletonSeparatedList( VariableDeclarator( Identifier( GetEventFieldBackingFieldName( symbol ) ) ) ) ) )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed, ElasticLineFeed )
                    .AddGeneratedCodeAnnotation();
        }

        private static MemberDeclarationSyntax GetOriginalImplEvent( EventDeclarationSyntax @event, IEventSymbol symbol )
        {
            return
                EventDeclaration(
                        List<AttributeListSyntax>(),
                        symbol.IsStatic
                            ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                        @event.Type,
                        null,
                        Identifier( GetOriginalImplMemberName( symbol.Name ) ),
                        null )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithAccessorList( @event.AccessorList )
                    .AddGeneratedCodeAnnotation();
        }

        private static MemberDeclarationSyntax GetOriginalImplEvent( EventFieldDeclarationSyntax eventField, IEventSymbol symbol )
        {
            return
                EventDeclaration(
                        List<AttributeListSyntax>(),
                        symbol.IsStatic
                            ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                        eventField.Declaration.Type,
                        null,
                        Identifier( GetOriginalImplMemberName( symbol.Name ) ),
                        AccessorList(
                            List(
                                new[]
                                {
                                    AccessorDeclaration( SyntaxKind.AddAccessorDeclaration, GetImplicitAdderBody( symbol.AddMethod.AssertNotNull() ) ),
                                    AccessorDeclaration(
                                        SyntaxKind.RemoveAccessorDeclaration,
                                        GetImplicitRemoverBody( symbol.RemoveMethod.AssertNotNull() ) )
                                } ) ) )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed, ElasticLineFeed )
                    .AddGeneratedCodeAnnotation();
        }
    }
}