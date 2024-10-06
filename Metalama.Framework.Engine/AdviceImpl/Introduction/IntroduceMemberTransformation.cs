// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Transformations;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal abstract class IntroduceMemberTransformation<T> : IntroduceDeclarationTransformation<T>
    where T : MemberBuilderData
{
    protected IntroduceMemberTransformation( Advice advice, T introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override TransformationObservability Observability
        => this.BuilderData.IsDesignTimeObservable ? TransformationObservability.Always : TransformationObservability.CompileTimeOnly;
}