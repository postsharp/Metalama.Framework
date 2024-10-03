// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceNamedTypeTransformation : IntroduceDeclarationTransformation<NamedTypeBuilder>
{
    public IntroduceNamedTypeTransformation( Advice advice, NamedTypeBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override TransformationObservability Observability => TransformationObservability.Always;

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var typeBuilder = this.IntroducedDeclaration;

        BaseListSyntax? baseList;

        if ( this.IntroducedDeclaration.BaseType != null && this.IntroducedDeclaration.BaseType.SpecialType != SpecialType.Object )
        {
            baseList = BaseList(
                SingletonSeparatedList<BaseTypeSyntax>(
                    SimpleBaseType( context.SyntaxGenerator.Type( this.IntroducedDeclaration.BaseType.ToNonNullableType() ) ) ) );
        }
        else
        {
            baseList = null;
        }

        var type =
            ClassDeclaration(
                    typeBuilder.GetAttributeLists( context ),
                    typeBuilder.GetSyntaxModifierList(),
                    Identifier( typeBuilder.Name ),
                    this.IntroducedDeclaration.TypeParameters.Count == 0
                        ? null
                        : TypeParameterList(
                            SeparatedList(
                                ((IEnumerable<TypeParameterBuilder>) this.IntroducedDeclaration.TypeParameters).Select(
                                    tp => TypeParameter( Identifier( tp.Name ) ) ) ) ),
                    baseList,
                    List<TypeParameterConstraintClauseSyntax>(),
                    List<MemberDeclarationSyntax>() )
                .NormalizeWhitespaceIfNecessary( context.SyntaxGenerationContext );

        switch ( typeBuilder.ContainingDeclaration )
        {
            case INamedType:
            case INamespace { IsGlobalNamespace: true }:
                return new[]
                {
                    new InjectedMember( this, type, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, this.IntroducedDeclaration )
                };

            case INamespace:
                var namespaceDeclaration =
                    NamespaceDeclaration(
                        Token( TriviaList(), SyntaxKind.NamespaceKeyword, TriviaList( ElasticSpace ) ),
                        ParseName( typeBuilder.ContainingNamespace.FullName ),
                        Token( TriviaList(), SyntaxKind.OpenBraceToken, TriviaList( context.SyntaxGenerationContext.ElasticEndOfLineTrivia ) ),
                        List<ExternAliasDirectiveSyntax>(),
                        List<UsingDirectiveSyntax>(),
                        SingletonList<MemberDeclarationSyntax>( type ),
                        Token( TriviaList( context.SyntaxGenerationContext.ElasticEndOfLineTrivia ), SyntaxKind.CloseBraceToken, TriviaList() ),
                        default );

                return new[]
                {
                    new InjectedMember(
                        this,
                        namespaceDeclaration,
                        this.ParentAdvice.AspectLayerId,
                        InjectedMemberSemantic.Introduction,
                        this.IntroducedDeclaration )
                };

            default:
                throw new AssertionFailedException( $"Unsupported containing declaration type '{typeBuilder.ContainingDeclaration.GetType()}'." );
        }
    }

    public override SyntaxTree TransformedSyntaxTree => this.IntroducedDeclaration.PrimarySyntaxTree;
}