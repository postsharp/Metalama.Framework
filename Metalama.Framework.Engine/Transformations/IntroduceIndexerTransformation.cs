// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class IntroduceIndexerTransformation : IntroduceMemberTransformation<IndexerBuilder>
{
    public IntroduceIndexerTransformation( Advice advice, IndexerBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override IEnumerable<InjectedMemberOrNamedType> GetInjectedMembers( MemberInjectionContext context )
    {
        var indexerBuilder = this.IntroducedDeclaration;
        var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

        var indexer =
            IndexerDeclaration(
                indexerBuilder.GetAttributeLists( context ),
                indexerBuilder.GetSyntaxModifierList(),
                syntaxGenerator.Type( indexerBuilder.Type.GetSymbol() ).WithOptionalTrailingTrivia( ElasticSpace, context.SyntaxGenerationContext.Options ),
                indexerBuilder.ExplicitInterfaceImplementations.Count > 0
                    ? ExplicitInterfaceSpecifier(
                        (NameSyntax) syntaxGenerator.Type( indexerBuilder.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
                    : null,
                Token( SyntaxKind.ThisKeyword ),
                context.SyntaxGenerator.ParameterList( indexerBuilder, context.Compilation ),
                GenerateAccessorList(),
                null,
                default );

        var injectedIndexer = new InjectedMemberOrNamedType(
            this,
            indexer,
            this.ParentAdvice.AspectLayerId,
            InjectedMemberSemantic.Introduction,
            indexerBuilder );

        return new[] { injectedIndexer };

        AccessorListSyntax GenerateAccessorList()
        {
            switch (indexerBuilder.Writeability, indexerBuilder.GetMethod, indexerBuilder.SetMethod)
            {
                // Indexers with both accessors.
                case (_, not null, not null):
                    return AccessorList( List( new[] { GenerateGetAccessor(), GenerateSetAccessor() } ) );

                // Indexers with only get accessor.
                case (_, not null, null):
                    return AccessorList( List( new[] { GenerateGetAccessor() } ) );

                // Indexers with only set accessor.
                case (_, null, not null):
                    return AccessorList( List( new[] { GenerateSetAccessor() } ) );

                default:
                    throw new AssertionFailedException( "Both the getter and the setter are undefined." );
            }
        }

        AccessorDeclarationSyntax GenerateGetAccessor()
        {
            var tokens = new List<SyntaxToken>();

            if ( indexerBuilder.GetMethod!.Accessibility != indexerBuilder.Accessibility )
            {
                indexerBuilder.GetMethod.Accessibility.AddTokens( tokens );
            }

            return
                AccessorDeclaration(
                    SyntaxKind.GetAccessorDeclaration,
                    indexerBuilder.GetAttributeLists( context, indexerBuilder.GetMethod ),
                    TokenList( tokens ),
                    Token( SyntaxKind.GetKeyword ),
                    syntaxGenerator.FormattedBlock(
                        ReturnStatement(
                            Token( TriviaList(), SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                            DefaultExpression( syntaxGenerator.Type( indexerBuilder.Type.GetSymbol() ) ),
                            Token( TriviaList(), SyntaxKind.SemicolonToken, context.SyntaxGenerationContext.ElasticEndOfLineTriviaList ) ) ),
                    null,
                    default );
        }

        AccessorDeclarationSyntax GenerateSetAccessor()
        {
            var tokens = new List<SyntaxToken>();

            if ( indexerBuilder.SetMethod!.Accessibility != indexerBuilder.Accessibility )
            {
                indexerBuilder.SetMethod.Accessibility.AddTokens( tokens );
            }

            return
                AccessorDeclaration(
                    indexerBuilder.HasInitOnlySetter ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration,
                    indexerBuilder.GetAttributeLists( context, indexerBuilder.SetMethod ),
                    TokenList( tokens ),
                    indexerBuilder.HasInitOnlySetter
                        ? Token( TriviaList(), SyntaxKind.InitKeyword, TriviaList( ElasticSpace ) )
                        : Token( TriviaList(), SyntaxKind.SetKeyword, TriviaList( ElasticSpace ) ),
                    context.SyntaxGenerator.FormattedBlock(),
                    null,
                    default );
        }
    }
}