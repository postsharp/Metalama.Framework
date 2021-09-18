// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Linking.Inlining;
using Caravela.Framework.Impl.Utilities;
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
        private IReadOnlyList<MemberDeclarationSyntax> RewriteEvent( EventDeclarationSyntax eventDeclaration, IEventSymbol symbol )
        {
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
                        members.Add( GetOriginalImplEventField( eventDeclaration.Type, symbol ) );
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
                var addDeclaration = (AccessorDeclarationSyntax) symbol.AddMethod.AssertNotNull().GetPrimaryDeclaration().AssertNotNull();

                var transformedAdd =
                    AccessorDeclaration(
                        SyntaxKind.AddAccessorDeclaration,
                        addDeclaration.AttributeLists,
                        TokenList(),
                        this.GetLinkedBody(
                            symbol.AddMethod.AssertNotNull().ToSemantic( semanticKind ),
                            InliningContext.Create( this, symbol.AddMethod.AssertNotNull() ) ) );

                var removeDeclaration = (AccessorDeclarationSyntax) symbol.RemoveMethod.AssertNotNull().GetPrimaryDeclaration().AssertNotNull();

                var transformedRemove =
                    AccessorDeclaration(
                        SyntaxKind.RemoveAccessorDeclaration,
                        removeDeclaration.AttributeLists,
                        TokenList(),
                        this.GetLinkedBody(
                            symbol.RemoveMethod.AssertNotNull().ToSemantic( semanticKind ),
                            InliningContext.Create( this, symbol.RemoveMethod.AssertNotNull() ) ) );

                return eventDeclaration
                    .WithAccessorList( AccessorList( List( new[] { transformedAdd, transformedRemove } ) ) )
                    .WithLeadingTrivia( eventDeclaration.GetLeadingTrivia() )
                    .WithTrailingTrivia( eventDeclaration.GetTrailingTrivia() );
            }
        }

        private static BlockSyntax GetImplicitAdderBody( IMethodSymbol symbol )
            => Block(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.AddAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            symbol.IsStatic
                                ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( symbol.ContainingType )
                                : ThisExpression(),
                            IdentifierName( GetBackingFieldName( (IEventSymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                        IdentifierName( "value" ) ) ) );

        private static BlockSyntax GetImplicitRemoverBody( IMethodSymbol symbol )
            => Block(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SubtractAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            symbol.IsStatic
                                ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( symbol.ContainingType )
                                : ThisExpression(),
                            IdentifierName( GetBackingFieldName( (IEventSymbol) symbol.AssociatedSymbol.AssertNotNull() ) ) ),
                        IdentifierName( "value" ) ) ) );

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
                .AddGeneratedCodeAnnotation();

        private static MemberDeclarationSyntax GetOriginalImplEvent( EventDeclarationSyntax @event, IEventSymbol symbol )
        {
            return GetSpecialImplEvent(
                @event.Type,
                @event.AccessorList.AssertNotNull().AddSourceCodeAnnotation(),
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
                    .AddGeneratedCodeAnnotation();
        }
    }
}