// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceNamespaceTransformation : IntroduceDeclarationTransformation<NamespaceBuilder>
{
    public IntroduceNamespaceTransformation( Advice advice, NamespaceBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override TransformationObservability Observability => TransformationObservability.Always;

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var @namespace = (INamespace) this.IntroducedDeclaration;

        yield return new InjectedMember(
            this,
            NamespaceDeclaration(
                Token( TriviaList(), SyntaxKind.NamespaceKeyword, TriviaList( ElasticSpace ) ),
                ParseName( @namespace.FullName ),
                Token( TriviaList(), SyntaxKind.OpenBraceToken, TriviaList( context.SyntaxGenerationContext.ElasticEndOfLineTrivia ) ),
                List<ExternAliasDirectiveSyntax>(),
                List<UsingDirectiveSyntax>(),
                List<MemberDeclarationSyntax>(),
                Token( TriviaList( context.SyntaxGenerationContext.ElasticEndOfLineTrivia ), SyntaxKind.CloseBraceToken, TriviaList() ),
                default ),
            this.ParentAdvice.AspectLayerId,
            InjectedMemberSemantic.Introduction,
            this.IntroducedDeclaration );
    }

    public override SyntaxTree TransformedSyntaxTree => this.IntroducedDeclaration.PrimarySyntaxTree;
}