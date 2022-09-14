﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Substitution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// Properties with overrides have the following structure:
//  * Final semantic. 
//  * Override n
//  * ...
//  * Override 1
//  * Default semantic.
//  * Base semantic (if the property was introduced).

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerRewritingDriver
    {
        public IReadOnlyList<MemberDeclarationSyntax> RewritePositionalProperty(
            ParameterSyntax recordParameter,
            IPropertySymbol symbol,
            SyntaxGenerationContext generationContext )
        {
            if ( this.IntroductionRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IPropertySymbol) this.IntroductionRegistry.GetLastOverride( symbol );

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) )
                     && this.AnalysisRegistry.IsInlined( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) ) )
                {
                    // Backing field for auto property.
                    members.Add( 
                        GetPropertyBackingField( 
                            recordParameter.Type.AssertNotNull(), 
                            EqualsValueClause( IdentifierName( recordParameter.Identifier.ValueText ) ),
                            FilterAttributeListsForTarget( recordParameter.AttributeLists, SyntaxKind.FieldKeyword, false, false ),
                            symbol ) );
                }

                if ( this.AnalysisRegistry.IsInlined( lastOverride.ToSemantic(IntermediateSymbolSemanticKind.Default) ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final ) );
                }
                else
                {
                    members.Add( GetTrampolineForPositionalProperty( recordParameter.Identifier, recordParameter.Type.AssertNotNull(), lastOverride ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) ) )
                {
                    members.Add( GetOriginalImplProperty( symbol, true, recordParameter.Type.AssertNotNull(), recordParameter.Default, null, null, generationContext ) );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic(IntermediateSymbolSemanticKind.Base) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic(IntermediateSymbolSemanticKind.Base) ) )
                {
                    members.Add( GetEmptyImplProperty( symbol, true, recordParameter.Type.AssertNotNull(), null ) );
                }

                return members;
            }
            else if ( this.IntroductionRegistry.IsOverride( symbol ) )
            {
                if ( !this.AnalysisRegistry.IsReachable( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) )
                     || this.AnalysisRegistry.IsInlined( symbol.ToSemantic(IntermediateSymbolSemanticKind.Default) ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[] { GetLinkedDeclaration( IntermediateSymbolSemanticKind.Default ) };
            }
            else
            {
                throw new AssertionFailedException();
            }

            MemberDeclarationSyntax GetLinkedDeclaration( IntermediateSymbolSemanticKind semanticKind )
            {
                var generatedAccessors = new List<AccessorDeclarationSyntax>
                {
                    GetLinkedAccessor( 
                        semanticKind, 
                        SyntaxKind.GetAccessorDeclaration, 
                        symbol.GetMethod.AssertNotNull() ),
                    GetLinkedAccessor( 
                        semanticKind, 
                        symbol.SetMethod.AssertNotNull().IsInitOnly
                            ? SyntaxKind.InitAccessorDeclaration
                            : SyntaxKind.SetAccessorDeclaration,
                        symbol.SetMethod.AssertNotNull() ),
                };

                return
                    PropertyDeclaration(
                        FilterAttributeListsForTarget( recordParameter.AttributeLists, SyntaxKind.PropertyKeyword, false, false ),
                        TokenList( Token( SyntaxKind.PublicKeyword ) ),
                        recordParameter.Type.AssertNotNull(),
                        null,
                        recordParameter.Identifier,
                        AccessorList( List( generatedAccessors ) ),
                        null,
                        null,
                        default )
                    .NormalizeWhitespace();
            }

            AccessorDeclarationSyntax GetLinkedAccessor(
                IntermediateSymbolSemanticKind semanticKind,
                SyntaxKind accessorSyntaxKind,                
                IMethodSymbol methodSymbol )
            {
                var linkedBody = this.GetSubstitutedBody(
                    methodSymbol.ToSemantic( semanticKind ),
                    new SubstitutionContext(
                        this,
                        generationContext,
                        new InliningContextIdentifier( methodSymbol.ToSemantic( semanticKind ) ) ) );

                var body =                             
                    linkedBody
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock )
                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );

                return
                    AccessorDeclaration(
                        accessorSyntaxKind,
                        List<AttributeListSyntax>(),
                        TokenList(),
                        Token(
                            accessorSyntaxKind switch
                            {
                                SyntaxKind.GetAccessorDeclaration => SyntaxKind.GetKeyword,
                                SyntaxKind.SetAccessorDeclaration => SyntaxKind.SetKeyword,
                                SyntaxKind.InitAccessorDeclaration => SyntaxKind.InitKeyword,
                                _ => throw new AssertionFailedException(),
                            } ),
                        null,
                        null,
                        default )
                    .NormalizeWhitespace()
                    .WithBody( body );
            }
        }

        private static PropertyDeclarationSyntax GetTrampolineForPositionalProperty( SyntaxToken identifier, TypeSyntax type, IPropertySymbol targetSymbol )
        {
            var getAccessor =
                AccessorDeclaration(
                    SyntaxKind.GetAccessorDeclaration,
                    Block(
                        ReturnStatement(
                            Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                            GetInvocationTarget(),
                            Token( SyntaxKind.SemicolonToken ) ) ) );

            var setAccessor =
                AccessorDeclaration(
                    targetSymbol.GetMethod.AssertNotNull().IsInitOnly ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration,
                    Block(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                GetInvocationTarget(),
                                IdentifierName( "value" ) ) ) ) );

            return
                PropertyDeclaration(
                    List<AttributeListSyntax>(),
                    TokenList( Token( SyntaxKind.PublicKeyword ) ),
                    type,
                    null,
                    identifier,
                    AccessorList( List( new[] { getAccessor, setAccessor } ) ),
                    null,
                    null,
                    default )
                .NormalizeWhitespace();

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