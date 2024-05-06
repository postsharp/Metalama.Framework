// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class IntroduceNamedTypeTransformation : IntroduceDeclarationTransformation<NamedTypeBuilder>
{
    public IntroduceNamedTypeTransformation( Advice advice, NamedTypeBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override TransformationObservability Observability => TransformationObservability.Always;

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var typeBuilder = this.IntroducedDeclaration;

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
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType( context.SyntaxGenerator.Type( this.IntroducedDeclaration.BaseType.AssertNotNull() ) ) ) ),
                    List<TypeParameterConstraintClauseSyntax>(),
                    List<MemberDeclarationSyntax>() )
                .NormalizeWhitespaceIfNecessary( context.SyntaxGenerationContext );

        return new[] { new InjectedMember( this, type, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, this.IntroducedDeclaration ) };
    }
}