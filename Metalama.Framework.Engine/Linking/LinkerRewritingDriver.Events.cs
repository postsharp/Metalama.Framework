﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Substitution;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerRewritingDriver
    {
        private IReadOnlyList<MemberDeclarationSyntax> RewriteEvent( EventDeclarationSyntax eventDeclaration, IEventSymbol symbol )
        {
            var generationContext = this.IntermediateCompilationContext.GetSyntaxGenerationContext( eventDeclaration );

            if ( this.InjectionRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IEventSymbol) this.InjectionRegistry.GetLastOverride( symbol );

                if ( eventDeclaration.GetLinkerDeclarationFlags().HasFlagFast( AspectLinkerDeclarationFlags.EventField )
                     && this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    // Backing field for event field.
                    members.Add( GetEventBackingField( eventDeclaration, symbol ) );
                }

                if ( this.AnalysisRegistry.IsInlined( lastOverride.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final ) );
                }
                else
                {
                    members.Add( GetTrampolineForEvent( eventDeclaration, lastOverride ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    if ( eventDeclaration.GetLinkerDeclarationFlags().HasFlagFast( AspectLinkerDeclarationFlags.EventField ) )
                    {
                        members.Add( GetOriginalImplEventField( eventDeclaration.Type, symbol ) );
                    }
                    else
                    {
                        members.Add( GetOriginalImplEvent( eventDeclaration, symbol ) );
                    }
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) ) )
                {
                    members.Add( GetEmptyImplEvent( eventDeclaration, symbol ) );
                }

                return members;
            }
            else
            {
                if ( eventDeclaration.GetLinkerDeclarationFlags().HasFlagFast( AspectLinkerDeclarationFlags.EventField ) )
                {
                    // Event field indicates explicit interface implementation with event field template.

                    return new MemberDeclarationSyntax[]
                    {
                        GetEventBackingField( eventDeclaration, symbol ),
                        GetLinkedDeclaration( IntermediateSymbolSemanticKind.Default ).NormalizeWhitespace()
                    };
                }

                if ( !this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) )
                     || this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
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
                    _ => throw new AssertionFailedException( $"Invalid accessor list at '{eventDeclaration.GetLocation()}'." )
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
                var linkedBody = this.GetSubstitutedBody(
                    methodSymbol.ToSemantic( semanticKind ),
                    new SubstitutionContext(
                        this,
                        generationContext,
                        new InliningContextIdentifier( methodSymbol.ToSemantic( semanticKind ) ) ) );

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
                        { ExpressionBody.ArrowToken: var arrowToken, SemicolonToken: var semicolonToken } =>
                            (arrowToken.LeadingTrivia.Add( ElasticLineFeed ), arrowToken.TrailingTrivia.Add( ElasticLineFeed ),
                             semicolonToken.LeadingTrivia.Add( ElasticLineFeed ), semicolonToken.TrailingTrivia),
                        _ => throw new AssertionFailedException( $"Unexpected accessor declaration at '{accessorDeclaration.GetLocation()}'." )
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
            => SyntaxFactoryEx.FormattedBlock(
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
            => SyntaxFactoryEx.FormattedBlock(
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
        {
            EqualsValueClauseSyntax? initializerExpression;

            switch ( eventDeclaration.GetLinkerDeclarationFlags() & AspectLinkerDeclarationFlags.HasInitializerExpressionMask )
            {
                case AspectLinkerDeclarationFlags.HasDefaultInitializerExpression:
                    initializerExpression =
                        EqualsValueClause(
                            LiteralExpression(
                                SyntaxKind.DefaultLiteralExpression,
                                Token( SyntaxKind.DefaultKeyword ) ) );

                    break;

                case AspectLinkerDeclarationFlags.HasHiddenInitializerExpression:
                    var firstStatement =
                        eventDeclaration.AccessorList.AssertNotNull()
                            .Accessors.First()
                            .Body.AssertNotNull()
                            .Statements.Single();

                    var expression = ((AssignmentExpressionSyntax) ((ExpressionStatementSyntax) firstStatement).Expression).Right;

                    initializerExpression = EqualsValueClause( expression );

                    break;

                default:
                    initializerExpression = null;

                    break;
            }

            return GetEventBackingField( eventDeclaration.Type, initializerExpression, symbol );
        }

        private static FieldDeclarationSyntax GetEventBackingField( TypeSyntax eventType, EqualsValueClauseSyntax? initializer, IEventSymbol symbol )
            => FieldDeclaration(
                    List<AttributeListSyntax>(),
                    symbol.IsStatic
                        ? TokenList(
                            Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ),
                            Token( SyntaxKind.StaticKeyword ).WithTrailingTrivia( Space ) )
                        : TokenList( Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ) ),
                    VariableDeclaration(
                        eventType.WithTrailingTrivia( Space ),
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier( GetBackingFieldName( symbol ) ),
                                null,
                                initializer ) ) ) )
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
                            ? TokenList(
                                Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ),
                                Token( SyntaxKind.StaticKeyword ).WithTrailingTrivia( Space ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ) ),
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

        private static EventDeclarationSyntax GetTrampolineForEvent( EventDeclarationSyntax @event, IEventSymbol targetSymbol )
        {
            var addAccessor = @event.AccessorList?.Accessors.SingleOrDefault( x => x.Kind() == SyntaxKind.AddAccessorDeclaration );
            var removeAccessor = @event.AccessorList?.Accessors.SingleOrDefault( x => x.Kind() == SyntaxKind.RemoveAccessorDeclaration );

            return @event
                .WithAccessorList(
                    AccessorList(
                        List(
                            new[]
                                {
                                    addAccessor != null
                                        ? AccessorDeclaration(
                                                SyntaxKind.AddAccessorDeclaration,
                                                SyntaxFactoryEx.FormattedBlock(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.AddAssignmentExpression,
                                                            GetInvocationTarget(),
                                                            IdentifierName( "value" ) ) ) ) )
                                            .NormalizeWhitespace()
                                        : null,
                                    removeAccessor != null
                                        ? AccessorDeclaration(
                                                SyntaxKind.RemoveAccessorDeclaration,
                                                SyntaxFactoryEx.FormattedBlock(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.SubtractAssignmentExpression,
                                                            GetInvocationTarget(),
                                                            IdentifierName( "value" ) ) ) ) )
                                            .NormalizeWhitespace()
                                        : null
                                }.Where( a => a != null )
                                .AssertNoneNull() ) ) )
                .WithLeadingTrivia( @event.GetLeadingTrivia() )
                .WithTrailingTrivia( @event.GetTrailingTrivia() );

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