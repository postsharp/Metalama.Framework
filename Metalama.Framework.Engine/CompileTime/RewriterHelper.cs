﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CompileTime;

internal sealed class RewriterHelper
{
    private readonly Func<SyntaxNode, SyntaxNode> _rewriteThrowNotSupported;

    public ISymbolClassifier SymbolClassifier { get; }

    public RewriterHelper(
        ClassifyingCompilationContext runTimeCompilationContext,
        Func<SyntaxNode, SyntaxNode>? rewriteThrowNotSupported = null )
    {
        this._rewriteThrowNotSupported = rewriteThrowNotSupported ?? (node => node);
        this.SymbolClassifier = runTimeCompilationContext.SymbolClassifier;
        this.SemanticModelProvider = runTimeCompilationContext.SemanticModelProvider;
    }

    public SemanticModelProvider SemanticModelProvider { get; }

    private T RewriteThrowNotSupported<T>( T node )
        where T : SyntaxNode
        => (T) this._rewriteThrowNotSupported( node );

    private static T WithSuppressedDiagnostics<T>( T member, params string[] suppressedDiagnostics )
        where T : MemberDeclarationSyntax
    {
        if ( suppressedDiagnostics.Length == 0 )
        {
            return member;
        }

        switch ( member )
        {
            case MethodDeclarationSyntax { Body: { } } method:
                return (T) (MemberDeclarationSyntax) method
                    .WithLeadingTrivia(
                        method
                            .GetLeadingTrivia()
                            .Add( Trivia( GetPragmaTrivia( true ) ) ) )
                    .WithTrailingTrivia(
                        TriviaList( Trivia( GetPragmaTrivia( false ) ) )
                            .AddRange( method.GetTrailingTrivia() ) );

            case MethodDeclarationSyntax { ExpressionBody: { } } method:
                return (T) (MemberDeclarationSyntax) method
                    .WithLeadingTrivia(
                        method
                            .GetLeadingTrivia()
                            .Add( Trivia( GetPragmaTrivia( true ) ) ) )
                    .WithTrailingTrivia(
                        TriviaList( Trivia( GetPragmaTrivia( false ) ) )
                            .AddRange( method.GetTrailingTrivia() ) );

            case MethodDeclarationSyntax method:
                return (T) (MemberDeclarationSyntax) method
                    .WithLeadingTrivia(
                        method
                            .GetLeadingTrivia()
                            .Add( Trivia( GetPragmaTrivia( true ) ) ) )
                    .WithTrailingTrivia(
                        TriviaList( Trivia( GetPragmaTrivia( false ) ) )
                            .AddRange( method.GetTrailingTrivia() ) );

            default:
                throw new AssertionFailedException( $"Unexpected member syntax kind {member.Kind()} at '{member.GetLocation()}'." );
        }

        StructuredTriviaSyntax GetPragmaTrivia( bool disable )
        {
            return PragmaWarningDirectiveTrivia(
                Token( SyntaxKind.HashToken ).WithLeadingTrivia( ElasticLineFeed ),
                Token( SyntaxKind.PragmaKeyword ).WithTrailingTrivia( ElasticSpace ),
                Token( SyntaxKind.WarningKeyword ).WithTrailingTrivia( ElasticSpace ),
                disable
                    ? Token( SyntaxKind.DisableKeyword ).WithTrailingTrivia( ElasticSpace )
                    : Token( SyntaxKind.RestoreKeyword ).WithTrailingTrivia( ElasticSpace ),
                SeparatedList<ExpressionSyntax>( suppressedDiagnostics.SelectAsReadOnlyList( IdentifierName ) ),
                Token( SyntaxKind.EndOfDirectiveToken ).WithTrailingTrivia( ElasticLineFeed ),
                true );
        }
    }

    public MethodDeclarationSyntax WithThrowNotSupportedExceptionBody( MethodDeclarationSyntax method, string message )
    {
        // Method does not have a body (e.g. because it's abstract) , so there is nothing to replace.
        if ( method.Body == null && method.ExpressionBody == null )
        {
            throw new ArgumentException( "Should not be called with a method without a body.", nameof(method) );
        }

        // Otherwise we need to preserve "asyncness" and "iteratorness" of the method.

        var isAsync = method.Modifiers.Any( x => x.IsKind( SyntaxKind.AsyncKeyword ) );
        var isIterator = IteratorHelper.IsIteratorMethod( method );

        var suppressedWarnings = new List<string>();

        if ( isAsync )
        {
            // Throwing async method does not have an await.
            suppressedWarnings.Add( "CS1998" );
        }

        if ( isIterator )
        {
            // Throwing iterator has unreachable yield break.
            suppressedWarnings.Add( "CS0162" );
        }

        return
            this.RewriteThrowNotSupported(
                WithSuppressedDiagnostics(
                    method
                        .WithBody(
                            isIterator
                                ? SyntaxFactoryEx.FormattedBlock(
                                    ThrowStatement( GetNotSupportedExceptionExpression( message ).Expression ),
                                    YieldStatement( SyntaxKind.YieldBreakStatement ) )
                                : null )
                        .WithExpressionBody(
                            isIterator
                                ? null
                                : ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                        .WithSemicolonToken( isIterator ? default : Token( SyntaxKind.SemicolonToken ) )
                        .NormalizeWhitespace()
                        .WithLeadingTrivia( method.GetLeadingTrivia() )
                        .WithTrailingTrivia( LineFeed, LineFeed ),
                    suppressedWarnings.ToArray() ) );
    }

    public BasePropertyDeclarationSyntax WithThrowNotSupportedExceptionBody( BasePropertyDeclarationSyntax memberDeclaration, string message )
    {
        if ( memberDeclaration.Modifiers.Any( x => x.IsKind( SyntaxKind.AbstractKeyword ) ) )
        {
            // Should not be called with an abstract property.
            throw new ArgumentOutOfRangeException( nameof(memberDeclaration) );
        }

        switch ( memberDeclaration )
        {
            case PropertyDeclarationSyntax { ExpressionBody: { } } property:
                // Expression bodied property - change the expression to throw exception.
                return this.RewriteThrowNotSupported(
                    property
                        .WithExpressionBody( property.ExpressionBody?.WithExpression( GetNotSupportedExceptionExpression( message ) ) )
                        .NormalizeWhitespace()
                        .WithLeadingTrivia( property.GetLeadingTrivia() )
                        .WithTrailingTrivia( LineFeed, LineFeed ) );

            case PropertyDeclarationSyntax { AccessorList: { } } property:
                // Property with accessor list - change all accessors to expression bodied which do throw exception, remove initializer.

                return this.RewriteThrowNotSupported(
                    property
                        .WithAccessorList(
                            property.AccessorList!.WithAccessors(
                                List(
                                    property.AccessorList.Accessors.SelectAsReadOnlyList(
                                        x => x
                                            .WithBody( null )
                                            .WithExpressionBody( ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                                            .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) ) ) ) )
                        .WithInitializer( null )
                        .NormalizeWhitespace()
                        .WithLeadingTrivia( property.GetLeadingTrivia() )
                        .WithTrailingTrivia( LineFeed, LineFeed ) );

            case IndexerDeclarationSyntax { AccessorList: { } }:
                throw new AssertionFailedException( "Build-time indexers are not supported." );
            /*
            // Property with accessor list - change all accessors to expression bodied which do throw exception, remove initializer.

            return indexer
                .WithAccessorList(
                    indexer.AccessorList!.WithAccessors(
                        List(
                            indexer.AccessorList.Accessors.Select(
                                x => x
                                    .WithBody( null )
                                    .WithExpressionBody( ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                                    .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) ) ) ) )
                .NormalizeWhitespace()
                .WithLeadingTrivia( indexer.GetLeadingTrivia() )
                .WithTrailingTrivia( LineFeed, LineFeed );
                */

            case EventDeclarationSyntax @event:
                // Event with accessor list.

                return this.RewriteThrowNotSupported(
                    @event
                        .WithAccessorList(
                            @event.AccessorList.AssertNotNull()
                                .WithAccessors(
                                    List(
                                        @event.AccessorList.AssertNotNull()
                                            .Accessors.SelectAsReadOnlyList(
                                                x => x
                                                    .WithBody( null )
                                                    .WithExpressionBody( ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                                                    .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) ) ) ) )
                        .NormalizeWhitespace()
                        .WithLeadingTrivia( @event.GetLeadingTrivia() )
                        .WithTrailingTrivia( LineFeed, LineFeed ) );

            default:
                throw new AssertionFailedException( $"Unexpected declaration syntax kind {memberDeclaration.Kind()} at '{memberDeclaration.GetLocation()}'." );
        }
    }

    /*
    protected static IEnumerable<EventDeclarationSyntax> WithThrowNotSupportedExceptionBody( EventFieldDeclarationSyntax @eventField, string message )
    {
        // Event with accessor list.

        foreach ( var declarator in @eventField.Declaration.Variables )
        {
            yield return
                EventDeclaration(
                    List<AttributeListSyntax>(),
                    eventField.Modifiers,
                    eventField.Declaration.Type,
                    null,
                    declarator.Identifier,
                    AccessorList(
                        List(
                            new[]
                            {
                                AccessorDeclaration(
                                    SyntaxKind.AddAccessorDeclaration,
                                    List<AttributeListSyntax>(),
                                    TokenList(),
                                    ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) ),
                                AccessorDeclaration(
                                    SyntaxKind.RemoveAccessorDeclaration,
                                    List<AttributeListSyntax>(),
                                    TokenList(),
                                    ArrowExpressionClause( GetNotSupportedExceptionExpression( message ) ) )
                            } ) ) );
        }
    }
    */

    private static ThrowExpressionSyntax GetNotSupportedExceptionExpression( string message )
        =>

            // throw new System.NotSupportedException("message")
            ThrowExpression(
                ObjectCreationExpression( ParseTypeName( "System.NotSupportedException" ) )
                    .AddArgumentListArguments( Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( message ) ) ) ) );
}