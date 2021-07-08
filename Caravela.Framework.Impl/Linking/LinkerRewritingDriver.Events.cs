// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
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
        private bool IsDiscarded( IEventSymbol symbol )
        {
            var addAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, AspectReferenceTargetKind.EventAddAccessor );
            var removeAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, AspectReferenceTargetKind.EventRemoveAccessor );

            if ( addAspectReferences.Count == 0 && removeAspectReferences.Count == 0 )
            {
                return true;
            }

            if ( this.IsInlineable( symbol ) )
            {
                return true;
            }

            return false;
        }

        private bool IsInlineable( IEventSymbol symbol )
        {
            if ( GetDeclarationFlags( symbol ).HasFlag( LinkerDeclarationFlags.NotInlineable) )
            {
                return false;
            }

            var addAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, AspectReferenceTargetKind.EventAddAccessor );
            var removeAspectReferences = this._analysisRegistry.GetAspectReferences( symbol, AspectReferenceTargetKind.EventRemoveAccessor );

            if ( addAspectReferences.Count > 1 || removeAspectReferences.Count > 1 )
            {
                return false;
            }

            return (addAspectReferences.Count == 0 || this.IsInlineableReference( addAspectReferences[0] ))
                && (removeAspectReferences.Count == 0 || this.IsInlineableReference( removeAspectReferences[0] ));
        }

        private IReadOnlyList<MemberDeclarationSyntax> RewriteEvent( EventDeclarationSyntax eventDeclaration, IEventSymbol symbol )
        {
            if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>
                {
                    GetLinkedDeclaration()
                };

                if ( !this.IsInlineable( symbol ) )
                {
                    members.Add( GetOriginalImplEvent( eventDeclaration ) );
                }

                if ( !this.IsInlineable( (IEventSymbol) this._analysisRegistry.GetLastOverride( symbol ) ) )
                {
                    members.Add( GetTrampolineEvent( eventDeclaration, symbol ) );
                }

                return members;
            }
            else
            {
                if ( this.IsDiscarded( symbol ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[]
                {
                    GetLinkedDeclaration()
                };
            }

            MemberDeclarationSyntax GetLinkedDeclaration()
            {
                var addDeclaration = (AccessorDeclarationSyntax)symbol.AddMethod.AssertNotNull().GetPrimaryDeclaration().AssertNotNull();

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
                var members = new List<MemberDeclarationSyntax>
                {
                    GetEventBackingField(eventFieldDeclaration, symbol),
                    GetLinkedDeclaration(),
                };

                if ( !this.IsInlineable( symbol ) )
                {
                    members.Add( GetOriginalImplEvent( eventFieldDeclaration, symbol ) );
                }

                if ( !this.IsInlineable( (IEventSymbol) this._analysisRegistry.GetLastOverride( symbol ) ) )
                {
                    members.Add( GetTrampolineEvent( eventFieldDeclaration, symbol ) );
                }

                return members;
            }
            else
            {
                if ( this.IsDiscarded( symbol ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[]
                {
                    GetLinkedDeclaration()
                };
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
                        eventFieldDeclaration.Declaration.Type,
                        null,
                        Identifier(symbol.Name),
                        AccessorList( List( new[] { transformedAdd, transformedRemove } ) ) )
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
                        SingletonSeparatedList(
                            VariableDeclarator( Identifier( GetEventFieldBackingFieldName( symbol ) ) ) ) ) );
        }

        private static MemberDeclarationSyntax GetOriginalImplEvent( EventDeclarationSyntax @event )
        {
            return @event
                .WithIdentifier( Identifier( GetOriginalImplMemberName( @event.Identifier.ValueText ) ) )
                .WithAccessorList( @event.AccessorList.AddSourceCodeAnnotation() );
        }

        private static MemberDeclarationSyntax GetOriginalImplEvent( EventFieldDeclarationSyntax eventField, IEventSymbol symbol )
        {
            return
                EventDeclaration(
                    eventField.AttributeLists,
                    eventField.Modifiers,
                    eventField.Declaration.Type,
                    null,
                    Identifier( GetOriginalImplMemberName( symbol.Name ) ),
                    AccessorList(
                        List(
                            new[]
                            {
                                AccessorDeclaration( SyntaxKind.AddAccessorDeclaration, GetImplicitAdderBody( symbol.AddMethod.AssertNotNull() ) ),
                                AccessorDeclaration( SyntaxKind.RemoveAccessorDeclaration, GetImplicitRemoverBody( symbol.RemoveMethod.AssertNotNull() ) ),
                            } ) ) );
        }
    }
}
