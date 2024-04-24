// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class IntroduceTypeTransformation : IntroduceMemberOrNamedTypeTransformation<NamedTypeBuilder>
{
    public IntroduceTypeTransformation( Advice advice, NamedTypeBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override TransformationObservability Observability => TransformationObservability.Always;

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var type =
            ClassDeclaration( this.IntroducedDeclaration.Name )
                .WithModifiers( this.IntroducedDeclaration.GetSyntaxModifierList() )
                .NormalizeWhitespaceIfNecessary(context.SyntaxGenerationContext);

        return new[]
        {
            new InjectedMember( this, type, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, this.IntroducedDeclaration )
        };
    }
}