// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Inlining;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerRewritingDriver
    {
        private IReadOnlyList<MemberDeclarationSyntax> RewriteEvent( EventDeclarationSyntax eventDeclaration, IEventSymbol symbol )
        {
            var generationContext = SyntaxGenerationContext.Create( this._serviceProvider, this._intermediateCompilation, eventDeclaration );

            if ( this._introductionRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IEventSymbol) this._introductionRegistry.GetLastOverride( symbol );

                if ( eventDeclaration.GetLinkerDeclarationFlags().HasFlag( LinkerDeclarationFlags.EventField )
                     && this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ) ) )
                {
                    // Backing field for event field.
                    members.Add( GetEventBackingField( eventDeclaration, symbol ) );
                }

                if ( this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( lastOverride, IntermediateSymbolSemanticKind.Default ), out _ ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final ) );
                }
                else
                {
                    members.Add( GetTrampolineEvent( eventDeclaration, lastOverride ) );
                }

                if ( this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ) )
                     && !this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ), out _ ) )
                {
                    if ( eventDeclaration.GetLinkerDeclarationFlags().HasFlag( LinkerDeclarationFlags.EventField ) )
                    {
                        members.Add( GetOriginalImplEventField( eventDeclaration.Type, symbol, generationContext ) );
                    }
                    else
                    {
                        members.Add( GetOriginalImplEvent( eventDeclaration, symbol ) );
                    }
                }

                if ( this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Base ) )
                     && !this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Base ), out _ ) )
                {
                    members.Add( GetEmptyImplEvent( eventDeclaration, symbol ) );
                }

                return members;
            }
            else
            {
                if ( eventDeclaration.GetLinkerDeclarationFlags().HasFlag( LinkerDeclarationFlags.EventField ) )
                {
                    // Event field indicates explicit interface implementation with event field template.

                    return new MemberDeclarationSyntax[]
                    {
                        GetEventBackingField( eventDeclaration, symbol ),
                        GetLinkedDeclaration( IntermediateSymbolSemanticKind.Default ).NormalizeWhitespace()
                    };
                }

                if ( !this._analysisRegistry.IsReachable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ) )
                     || this._analysisRegistry.IsInlineable( new IntermediateSymbolSemantic( symbol, IntermediateSymbolSemanticKind.Default ), out _ ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[] { GetLinkedDeclaration( IntermediateSymbolSemanticKind.Default ) };
            }

            EventDeclarationSyntax GetLinkedDeclaration( IntermediateSymbolSemanticKind semanticKind )
            {
                var addAccessorDeclaration = (AccessorDeclarationSyntax) symbol.AddMethod.AssertNotNull().GetPrimaryDeclaration().AssertNotNull();
                var removeAccessorDeclaration = (AccessorDeclarationSyntax) symbol.RemoveMethod.AssertNotNull().GetPrimaryDeclaration().AssertNotNull();

                var transformedAdd = GetLinkedAccessor( semanticKind, addAccessorDeclaration, symbol.AddMethod.AssertNotNull() );
                var transformedRemove = GetLinkedAccessor( semanticKind, removeAccessorDeclaration, symbol.RemoveMethod.AssertNotNull() );

                var (accessorListLeadingTrivia, accessorStartingTrivia, accessorEndingTrivia, accessorListTrailingTrivia) = eventDeclaration switch
                {
                    { AccessorList: not null and var accessorList } => (
                        accessorList.OpenBraceToken.LeadingTrivia,
                        accessorList.OpenBraceToken.TrailingTrivia,
                        accessorList.CloseBraceToken.LeadingTrivia,
                        accessorList.CloseBraceToken.TrailingTrivia),
                    _ => throw new AssertionFailedException()
                };

                return eventDeclaration
                    .WithAccessorList(
                        AccessorList(
                            Token( accessorListLeadingTrivia, SyntaxKind.OpenBraceToken, accessorStartingTrivia ),
                            List( new[] { transformedAdd, transformedRemove } ),
                            Token( accessorEndingTrivia, SyntaxKind.CloseBraceToken, accessorListTrailingTrivia ) ) );
            }

            AccessorDeclarationSyntax GetLinkedAccessor(
                IntermediateSymbolSemanticKind semanticKind,
                AccessorDeclarationSyntax accessorDeclaration,
                IMethodSymbol methodSymbol )
            {
                var linkedBody =
                    this.GetLinkedBody(
                        methodSymbol.ToSemantic( semanticKind ),
                        InliningContext.Create( this, methodSymbol, generationContext ) );

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

        private static BlockSyntax GetImplicitAdderBody( IMethodSymbol symbol, SyntaxGenerationContext generationContext )
            => Block(
                ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.AddAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                symbol.IsStatic
                                    ? generationContext.SyntaxGenerator.Type( symbol.ContainingType )
                                    : ThisExpression(),
                                IdentifierName( GetBackingFieldName( (IEventSymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                            IdentifierName( "value" ) ) )
                    .WithTrailingTrivia( TriviaList( ElasticLineFeed ) ) );

        private static BlockSyntax GetImplicitRemoverBody( IMethodSymbol symbol, SyntaxGenerationContext generationContext )
            => Block(
                ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SubtractAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                symbol.IsStatic
                                    ? generationContext.SyntaxGenerator.Type( symbol.ContainingType )
                                    : ThisExpression(),
                                IdentifierName( GetBackingFieldName( (IEventSymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                            IdentifierName( "value" ) ) )
                    .WithTrailingTrivia( TriviaList( ElasticLineFeed ) ) );

        private static FieldDeclarationSyntax GetEventBackingField( EventDeclarationSyntax eventDeclaration, IEventSymbol symbol )
            => GetEventBackingField( eventDeclaration.Type, symbol );

        private static FieldDeclarationSyntax GetEventBackingField( TypeSyntax eventType, IEventSymbol symbol )
            => FieldDeclaration(
                    List<AttributeListSyntax>(),
                    symbol.IsStatic
                        ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                        : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                    VariableDeclaration(
                        eventType,
                        SingletonSeparatedList( VariableDeclarator( Identifier( GetBackingFieldName( symbol ) ) ) ) ) )
                .NormalizeWhitespace()
                .WithLeadingTrivia( ElasticLineFeed )
                .WithTrailingTrivia( ElasticLineFeed, ElasticLineFeed )
                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );

        private static MemberDeclarationSyntax GetOriginalImplEvent( EventDeclarationSyntax @event, IEventSymbol symbol )
        {
            return GetSpecialImplEvent(
                @event.Type,
                @event.AccessorList.AssertNotNull().WithSourceCodeAnnotation(),
                symbol,
                GetOriginalImplMemberName( symbol ) );
        }

        private static MemberDeclarationSyntax GetEmptyImplEvent( EventDeclarationSyntax @event, IEventSymbol symbol )
        {
            return GetSpecialImplEvent( @event.Type, @event.AccessorList.AssertNotNull(), symbol, GetEmptyImplMemberName( symbol ) );
        }

        private static MemberDeclarationSyntax GetSpecialImplEvent( TypeSyntax eventType, AccessorListSyntax accessorList, IEventSymbol symbol, string name )
        {
            return
                EventDeclaration(
                        List<AttributeListSyntax>(),
                        symbol.IsStatic
                            ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                        eventType,
                        null,
                        Identifier( name ),
                        null )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithAccessorList( accessorList )
                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );
        }
    }
}