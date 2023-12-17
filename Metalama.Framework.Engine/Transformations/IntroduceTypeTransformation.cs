// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class IntroduceTypeTransformation : IntroduceMemberOrNamedTypeTransformation<TypeBuilder>
{
    public IntroduceTypeTransformation( Advice advice, TypeBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override TransformationObservability Observability => TransformationObservability.None;

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var type =
            ClassDeclaration( this.IntroducedDeclaration.Name );

        return new[] { new InjectedMember( this, type, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, this.IntroducedDeclaration ) };
    }
}