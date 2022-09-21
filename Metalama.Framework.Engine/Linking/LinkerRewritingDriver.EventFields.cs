﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Substitution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
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

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) )
                     && this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    members.Add( GetEventBackingField( eventFieldDeclaration, symbol ) );
                }

                if ( this.AnalysisRegistry.IsInlined( lastOverride.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final ) );
                }
                else
                {
                    members.Add( GetTrampolineForEventField( eventFieldDeclaration, lastOverride ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    members.Add( GetOriginalImplEventField( eventFieldDeclaration.Declaration.Type, symbol ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) ) )
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

        private static MemberDeclarationSyntax GetOriginalImplEventField( TypeSyntax eventType, IEventSymbol symbol )
        {
            return GetSpecialImplEventField( eventType, symbol, GetOriginalImplMemberName( symbol ) );
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

        private static MemberDeclarationSyntax GetSpecialImplEventField( TypeSyntax eventType, IEventSymbol symbol, string name )
        {
            return
                FieldDeclaration(
                        List<AttributeListSyntax>(),
                        symbol.IsStatic
                            ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                        VariableDeclaration(
                            eventType,
                            SingletonSeparatedList( VariableDeclarator( Identifier( name ) ) ) ),
                        Token( TriviaList(), SyntaxKind.SemicolonToken, TriviaList( ElasticLineFeed ) ) )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );
        }

        private static EventDeclarationSyntax GetTrampolineForEventField( EventFieldDeclarationSyntax eventField, IEventSymbol targetSymbol )
        {
            // TODO: Do not copy leading/trailing trivia to all declarations.

            return
                EventDeclaration(
                        List<AttributeListSyntax>(),
                        eventField.Modifiers,
                        Token( SyntaxKind.EventKeyword ).WithTrailingTrivia( ElasticSpace ),
                        eventField.Declaration.Type,
                        null,
                        eventField.Declaration.Variables.Single().Identifier,
                        AccessorList(
                            List(
                                new[]
                                    {
                                        AccessorDeclaration(
                                                SyntaxKind.AddAccessorDeclaration,
                                                Block(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.AddAssignmentExpression,
                                                            GetInvocationTarget(),
                                                            IdentifierName( "value" ) ) ) ) )
                                            .NormalizeWhitespace(),
                                        AccessorDeclaration(
                                                SyntaxKind.RemoveAccessorDeclaration,
                                                Block(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.SubtractAssignmentExpression,
                                                            GetInvocationTarget(),
                                                            IdentifierName( "value" ) ) ) ) )
                                            .NormalizeWhitespace()
                                    }.Where( a => a != null )
                                    .AssertNoneNull() ) ),
                        default )
                    .WithLeadingTrivia( eventField.GetLeadingTrivia() )
                    .WithTrailingTrivia( eventField.GetTrailingTrivia() );

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