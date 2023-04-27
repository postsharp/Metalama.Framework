// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Linking.Substitution;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LinkerRewritingDriver
    {
        private IReadOnlyList<MemberDeclarationSyntax> RewriteEventField( EventFieldDeclarationSyntax eventFieldDeclaration, IEventSymbol symbol )
        {
            var generationContext = this.IntermediateCompilationContext.GetSyntaxGenerationContext( eventFieldDeclaration );

            if ( this.InjectionRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IEventSymbol) this.InjectionRegistry.GetLastOverride( symbol );

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
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

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                     && this.ShouldGenerateEmptyMember( symbol ) )
                {
                    members.Add( this.GetEmptyImplEventField( eventFieldDeclaration.Declaration.Type, symbol ) );
                }

                return members;
            }
            else
            {
                var members = new List<MemberDeclarationSyntax>();

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                     && this.ShouldGenerateEmptyMember( symbol ) )
                {
                    members.Add(
                        this.GetEmptyImplEventField(
                            eventFieldDeclaration.Declaration.Type,
                            symbol ) );
                }

                members.Add( eventFieldDeclaration );

                return members;
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
                        FilterAttributeListsForTarget( eventFieldDeclaration.AttributeLists, SyntaxKind.EventKeyword, true, true ),
                        eventFieldDeclaration.Modifiers,
                        Token( TriviaList(), SyntaxKind.EventKeyword, TriviaList( Space ) ),
                        eventFieldDeclaration.Declaration.Type.WithTrailingTrivia( Space ),
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
                        FilterAttributeListsForTarget( eventFieldDeclaration.AttributeLists, SyntaxKind.MethodKeyword, false, false )
                            .AddRange( FilterAttributeListsForTarget( eventFieldDeclaration.AttributeLists, SyntaxKind.Parameter, false, true ) ),
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

        private static EventFieldDeclarationSyntax GetEventBackingField( EventFieldDeclarationSyntax eventFieldDeclaration, IEventSymbol symbol )
        {
            var declarator = (VariableDeclaratorSyntax) symbol.GetPrimaryDeclaration().AssertNotNull();

            return
                GetEventBackingField(
                    eventFieldDeclaration.Declaration.Type,
                    declarator.Initializer,
                    symbol );
        }

        private MemberDeclarationSyntax GetEmptyImplEventField( TypeSyntax eventType, IEventSymbol symbol )
        {
            var accessorList =
                AccessorList(
                        List(
                            new[]
                            {
                                AccessorDeclaration( SyntaxKind.AddAccessorDeclaration, SyntaxFactoryEx.FormattedBlock() ),
                                AccessorDeclaration( SyntaxKind.RemoveAccessorDeclaration, SyntaxFactoryEx.FormattedBlock() )
                            } ) )
                    .NormalizeWhitespace();

            return this.GetSpecialImplEvent( eventType, accessorList, symbol, GetEmptyImplMemberName( symbol ) );
        }

        private static EventDeclarationSyntax GetTrampolineForEventField( EventFieldDeclarationSyntax eventField, IEventSymbol targetSymbol )
        {
            // TODO: Do not copy leading/trailing trivia to all declarations.

            return
                EventDeclaration(
                        List<AttributeListSyntax>(),
                        eventField.Modifiers,
                        Token( SyntaxKind.EventKeyword ).WithTrailingTrivia( Space ),
                        eventField.Declaration.Type.WithTrailingTrivia( Space ),
                        null,
                        eventField.Declaration.Variables.Single().Identifier,
                        AccessorList(
                            List(
                                new[]
                                {
                                    AccessorDeclaration(
                                            SyntaxKind.AddAccessorDeclaration,
                                            SyntaxFactoryEx.FormattedBlock(
                                                ExpressionStatement(
                                                    AssignmentExpression(
                                                        SyntaxKind.AddAssignmentExpression,
                                                        GetInvocationTarget(),
                                                        IdentifierName( "value" ) ) ) ) )
                                        .NormalizeWhitespace(),
                                    AccessorDeclaration(
                                            SyntaxKind.RemoveAccessorDeclaration,
                                            SyntaxFactoryEx.FormattedBlock(
                                                ExpressionStatement(
                                                    AssignmentExpression(
                                                        SyntaxKind.SubtractAssignmentExpression,
                                                        GetInvocationTarget(),
                                                        IdentifierName( "value" ) ) ) ) )
                                        .NormalizeWhitespace()
                                }.WhereNotNull() ) ),
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