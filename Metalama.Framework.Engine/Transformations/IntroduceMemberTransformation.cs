// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class IntroduceMemberTransformation<T> : IntroduceMemberOrNamedTypeTransformation<T>
    where T : MemberBuilder
{
    protected IntroduceMemberTransformation( Advice advice, T introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override TransformationObservability Observability
        => this.IntroducedDeclaration.IsDesignTime ? TransformationObservability.Always : TransformationObservability.CompileTimeOnly;
}