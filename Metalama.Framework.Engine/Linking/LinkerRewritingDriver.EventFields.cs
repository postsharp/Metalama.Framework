// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Linking.Inlining;
using Metalama.Framework.Engine.Linking.Substitution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerRewritingDriver
    {
        private IReadOnlyList<MemberDeclarationSyntax> RewriteEventField( EventFieldDeclarationSyntax eventFieldDeclaration, IEventSymbol symbol )
        {
            var generationContext = SyntaxGenerationContext.Create( this.ServiceProvider, this.IntermediateCompilation, eventFieldDeclaration );

            if ( this.IntroductionRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IEventSymbol) this.IntroductionRegistry.GetLastOverride( symbol );

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) ) )
                {
                    members.Add( GetEventBackingField( eventFieldDeclaration, symbol ) );
                }

                if ( this.AnalysisRegistry.IsInlined( lastOverride.ToSemantic(IntermediateSymbolSemanticKind.Default) ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final ) );
                }
                else
                {
                    members.Add( GetTrampolineEvent( eventFieldDeclaration, lastOverride ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) ) )
                {
                    members.Add( GetOriginalImplEventField( eventFieldDeclaration.Declaration.Type, symbol, generationContext ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic(IntermediateSymbolSemanticKind.Base) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic(IntermediateSymbolSemanticKind.Base) ) )
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
                var transformedAdd = GetLinkedAccessor(
                    semanticKind,
                    SyntaxKind.AddAccessorDeclaration,
                    SyntaxKind.AddKeyword,
                    symbol.AddMethod.AssertNotNull() );

                var transformedRemove = GetLinkedAccessor(
                    semanticKind,
                    SyntaxKind.RemoveAccessorDeclaration,
                    SyntaxKind.RemoveKeyword,
                    symbol.RemoveMethod.AssertNotNull() );

                return
                    EventDeclaration(
                        List<AttributeListSyntax>(),
                        eventFieldDeclaration.Modifiers,
                        Token( TriviaList(), SyntaxKind.EventKeyword, TriviaList( ElasticSpace ) ),
                        eventFieldDeclaration.Declaration.Type,
                        null,
                        Identifier( symbol.Name ),
                        AccessorList( List( new[] { transformedAdd, transformedRemove } ) )
                            .WithOpenBraceToken( Token( TriviaList( ElasticLineFeed ), SyntaxKind.OpenBraceToken, TriviaList( ElasticLineFeed ) ) )
                            .WithCloseBraceToken( Token( TriviaList( ElasticMarker ), SyntaxKind.CloseBraceToken, TriviaList( ElasticLineFeed ) ) ),
                        MissingToken( SyntaxKind.SemicolonToken ) );
            }

            AccessorDeclarationSyntax GetLinkedAccessor(
                IntermediateSymbolSemanticKind semanticKind,
                SyntaxKind accessorKind,
                SyntaxKind accessorKeyword,
                IMethodSymbol methodSymbol )
            {
                var linkedBody = this.GetSubstitutedBody(
                    methodSymbol.ToSemantic( semanticKind ),
                    new SubstitutionContext(
                        this,
                        generationContext,
                        new InliningContextIdentifier( methodSymbol.ToSemantic( semanticKind ) ) ) );

                var (openBraceLeadingTrivia, openBraceTrailingTrivia, closeBraceLeadingTrivia, closeBraceTrailingTrivia) =
                    (TriviaList(), TriviaList( ElasticLineFeed ), TriviaList( ElasticMarker ), TriviaList( ElasticLineFeed ));

                return
                    AccessorDeclaration(
                        accessorKind,
                        List<AttributeListSyntax>(),
                        TokenList(),
                        Token( TriviaList(), accessorKeyword, TriviaList( ElasticLineFeed ) ),
                        Block( linkedBody )
                            .WithOpenBraceToken( Token( openBraceLeadingTrivia, SyntaxKind.OpenBraceToken, openBraceTrailingTrivia ) )
                            .WithCloseBraceToken( Token( closeBraceLeadingTrivia, SyntaxKind.CloseBraceToken, closeBraceTrailingTrivia ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                        null,
                        default );
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
                                AccessorDeclaration(
                                    SyntaxKind.AddAccessorDeclaration,
                                    GetImplicitAdderBody( symbol.AddMethod.AssertNotNull(), generationContext ) ),
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