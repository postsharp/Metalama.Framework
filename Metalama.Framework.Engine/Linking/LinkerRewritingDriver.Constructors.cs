// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Substitution;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Metalama.Framework.Engine.Templating.SyntaxFactoryEx;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LinkerRewritingDriver
    {
        private IReadOnlyList<MemberDeclarationSyntax> RewriteConstructor(
            ConstructorDeclarationSyntax constructorDeclaration,
            IMethodSymbol symbol,
            SyntaxGenerationContext generationContext )
        {
            var members = new List<MemberDeclarationSyntax>();

            // If deconstructing primary constructor, add all fields defined by primary constructor parameters.
            if ( this.InjectionRegistry.IsAuxiliarySourceSymbol( symbol )
                 && this.LateTransformationRegistry.HasRemovedPrimaryConstructor( symbol.ContainingType ) )
            {
                foreach ( var primaryConstructorField in this.LateTransformationRegistry.GetPrimaryConstructorFields( symbol.ContainingType ) )
                {
                    members.Add(
                        FieldDeclaration(
                             List<AttributeListSyntax>(),
                             TokenList( TokenWithTrailingSpace( SyntaxKind.PrivateKeyword ), TokenWithTrailingSpace( SyntaxKind.ReadOnlyKeyword ) ),
                             VariableDeclaration(
                                 generationContext.SyntaxGenerator
                                    .Type( primaryConstructorField.Type )
                                    .WithTrailingTriviaIfNecessary( ElasticSpace, generationContext.PreserveTrivia ),
                                 SingletonSeparatedList(
                                     VariableDeclarator(
                                         Identifier( TriviaList( ElasticSpace ), primaryConstructorField.Name[1..^2], default ) ) ) ),
                             TokenWithTrailingLineFeed( SyntaxKind.SemicolonToken ) ) );
                }
            }

            if ( this.InjectionRegistry.IsOverrideTarget( symbol ) )
            {
                var lastOverride = this.InjectionRegistry.GetLastOverride( symbol );

                if ( this.AnalysisRegistry.IsInlined( lastOverride.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Final ) );
                }
                else
                {
                    throw new AssertionFailedException( "Uninlined constructors are not supported." );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) )
                     && this.ShouldGenerateSourceMember( symbol ) )
                {
                    throw new AssertionFailedException( "Uninlined constructors are not supported." );
                }

                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                     && this.ShouldGenerateEmptyMember( symbol ) )
                {
                    throw new AssertionFailedException( "Uninlined constructors are not supported." );
                }
            }
            else if ( this.InjectionRegistry.IsOverride( symbol ) )
            {
                if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) )
                     && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) ) )
                {
                    members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Default ) );
                }
            }
            else if ( this.AnalysisRegistry.HasAnySubstitutions( symbol ) )
            {
                members.Add( GetLinkedDeclaration( IntermediateSymbolSemanticKind.Default ) );
            }
            else
            {
                members.Add( constructorDeclaration );
            }

            return members;

            ConstructorDeclarationSyntax GetLinkedDeclaration( IntermediateSymbolSemanticKind semanticKind )
            {
                var linkedBody = this.GetSubstitutedBody(
                    symbol.ToSemantic( semanticKind ),
                    new SubstitutionContext(
                        this,
                        generationContext,
                        new InliningContextIdentifier( symbol.ToSemantic( semanticKind ) ) ) );

                var (openBraceLeadingTrivia, openBraceTrailingTrivia, closeBraceLeadingTrivia, closeBraceTrailingTrivia) =
                    constructorDeclaration switch
                    {
                        { Body: { OpenBraceToken: var openBraceToken, CloseBraceToken: var closeBraceToken } } =>
                            (openBraceToken.LeadingTrivia, openBraceToken.TrailingTrivia, closeBraceToken.LeadingTrivia, closeBraceToken.TrailingTrivia),
                        { ExpressionBody.ArrowToken: var arrowToken, SemicolonToken: var semicolonToken } =>
                            (arrowToken.LeadingTrivia.Add( ElasticLineFeed ), arrowToken.TrailingTrivia.Add( ElasticLineFeed ),
                             semicolonToken.LeadingTrivia.Add( ElasticLineFeed ), semicolonToken.TrailingTrivia),
                        _ => throw new AssertionFailedException( $"Unsupported form of constructor declaration for {symbol}." )
                    };

                var isAuxiliaryForPrimaryConstructor = this.InjectionRegistry.IsAuxiliarySourceSymbol( symbol );

                if ( isAuxiliaryForPrimaryConstructor )
                {
                    List<StatementSyntax> primaryConstructorFieldAssignments = new();

                    foreach ( var primaryConstructorField in this.LateTransformationRegistry.GetPrimaryConstructorFields( symbol.ContainingType ) )
                    {
                        var cleanName = primaryConstructorField.Name[1..^2];

                        primaryConstructorFieldAssignments.Add(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName( cleanName ) ),
                                    IdentifierName( cleanName ) ) ) );
                    }

                    foreach ( var member in symbol.ContainingType.GetMembers() )
                    {
                        if ( !this.LateTransformationRegistry.IsPrimaryConstructorInitializedMember( member ) )
                        {
                            continue;
                        }

                        string name;
                        ExpressionSyntax expression;

                        switch ( member )
                        {
                            case IFieldSymbol field:
                                var fieldDeclaration = (VariableDeclaratorSyntax) field.GetPrimaryDeclaration().AssertNotNull();

                                name = field.Name;
                                expression = fieldDeclaration.Initializer.AssertNotNull().Value;

                                break;

                            case IEventSymbol eventField:
                                var eventFieldDeclaration = (VariableDeclaratorSyntax) eventField.GetPrimaryDeclaration().AssertNotNull();

                                name = eventField.Name;
                                expression = eventFieldDeclaration.Initializer.AssertNotNull().Value;

                                break;

                            case IPropertySymbol property:
                                var propertyDeclaration = (PropertyDeclarationSyntax) property.GetPrimaryDeclaration().AssertNotNull();

                                name = propertyDeclaration.Identifier.ValueText;
                                expression = propertyDeclaration.Initializer.AssertNotNull().Value;

                                break;

                            default:
                                throw new AssertionFailedException( $"Unsupported: {member}" );
                        }

                        primaryConstructorFieldAssignments.Add(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName( name ) ),
                                    expression ) ) );
                    }

                    if ( primaryConstructorFieldAssignments.Count > 0 )
                    {
                        linkedBody = 
                            Block(
                                Block( primaryConstructorFieldAssignments ).WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                                linkedBody )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                    }
                }

                var ret = constructorDeclaration.PartialUpdate(
                    expressionBody: null,
                    body: Block(
                            Token( openBraceLeadingTrivia, SyntaxKind.OpenBraceToken, openBraceTrailingTrivia ),
                            SingletonList<StatementSyntax>( linkedBody ),
                            Token( closeBraceLeadingTrivia, SyntaxKind.CloseBraceToken, closeBraceTrailingTrivia ) )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock )
                        .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ),
                    parameterList:
                        isAuxiliaryForPrimaryConstructor
                        ? constructorDeclaration.ParameterList.WithParameters(
                            constructorDeclaration.ParameterList.Parameters.RemoveAt( constructorDeclaration.ParameterList.Parameters.Count - 1 ) )
                        : default,
                    initializer:
                        isAuxiliaryForPrimaryConstructor
                        ? this.LateTransformationRegistry.GetPrimaryConstructorBaseArgumentList( symbol ) switch
                        {
                            { } arguments => ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, arguments),
                            null => null,
                        }
                        : default,
                    semicolonToken: default( SyntaxToken ) );

                return ret;
            }
        }
    }
}