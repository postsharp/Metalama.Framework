﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking.Inlining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerRewritingDriver
    {
        private IReadOnlyList<MemberDeclarationSyntax> RewriteEventField( EventFieldDeclarationSyntax eventFieldDeclaration, IEventSymbol symbol )
        {
            var generationContext = SyntaxGenerationContext.Create( this.IntermediateCompilation, eventFieldDeclaration );

            
            if ( this._introductionRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IEventSymbol) this._introductionRegistry.GetLastOverride( symbol );

                if ( this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ) ) )
                {
                    members.Add( GetEventBackingField( eventFieldDeclaration, symbol ) );
                }

                if ( this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( lastOverride, IntermediateSymbolSemanticKind.Default ), out _ ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final ) );
                }
                else
                {
                    members.Add( GetTrampolineEvent( eventFieldDeclaration, lastOverride ) );
                }

                if ( this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ) )
                     && !this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ), out _ ) )
                {
                    
                    members.Add( GetOriginalImplEventField( eventFieldDeclaration.Declaration.Type, symbol, generationContext ) );
                }

                if ( this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Base ) )
                     && !this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Base ), out _ ) )
                {
                    members.Add( GetEmptyImplEventField( eventFieldDeclaration.Declaration.Type, symbol ) );
                }

                return members;
            }
            else
            {
                throw new AssertionFailedException();
            }

            MemberDeclarationSyntax GetLinkedDeclaration( IntermediateSymbolSemanticKind semanticKind )
            {
                var transformedAdd =
                    AccessorDeclaration(
                        SyntaxKind.AddAccessorDeclaration,
                        List<AttributeListSyntax>(),
                        TokenList(),
                        this.GetLinkedBody(
                            symbol.AddMethod.AssertNotNull().ToSemantic( semanticKind ),
                            InliningContext.Create( this, symbol.AddMethod.AssertNotNull(), generationContext ) ) );

                var transformedRemove =
                    AccessorDeclaration(
                        SyntaxKind.RemoveAccessorDeclaration,
                        List<AttributeListSyntax>(),
                        TokenList(),
                        this.GetLinkedBody(
                            symbol.RemoveMethod.AssertNotNull().ToSemantic( semanticKind ),
                            InliningContext.Create( this, symbol.RemoveMethod.AssertNotNull(), generationContext ) ) );

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

        private static FieldDeclarationSyntax GetEventBackingField( EventFieldDeclarationSyntax eventFieldDeclaration, IEventSymbol symbol )
            => GetEventBackingField( eventFieldDeclaration.Declaration.Type, symbol );

        private static MemberDeclarationSyntax GetOriginalImplEventField( TypeSyntax eventType, IEventSymbol symbol, SyntaxGenerationContext generationContext )
        {
            var accessorList =
                AccessorList(
                        List(
                            new[]
                            {
                                AccessorDeclaration( SyntaxKind.AddAccessorDeclaration, GetImplicitAdderBody( symbol.AddMethod.AssertNotNull(), generationContext ) ),
                                AccessorDeclaration(
                                    SyntaxKind.RemoveAccessorDeclaration,
                                    GetImplicitRemoverBody( symbol.RemoveMethod.AssertNotNull(), generationContext ) )
                            } ) )
                    .NormalizeWhitespace();

            return GetSpecialImplEvent( eventType, accessorList, symbol, GetOriginalImplMemberName( symbol ) );
        }

        private static MemberDeclarationSyntax GetEmptyImplEventField( TypeSyntax eventType, IEventSymbol symbol )
        {
            var accessorList =
                AccessorList(
                        List(
                            new[]
                            {
                                AccessorDeclaration( SyntaxKind.AddAccessorDeclaration, Block() ),
                                AccessorDeclaration( SyntaxKind.RemoveAccessorDeclaration, Block() )
                            } ) )
                    .NormalizeWhitespace();

            return GetSpecialImplEvent( eventType, accessorList, symbol, GetEmptyImplMemberName( symbol ) );
        }
    }
}