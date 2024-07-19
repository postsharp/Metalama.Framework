// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Override;
// TODO: Check why this class is unused.
// ReSharper disable once UnusedType.Global

// Because we are using OverrideMethodAdvice, but that does not return a correct AdviceKind (#34372).

internal class OverrideFinalizerAdvice : OverrideMemberAdvice<IMethod, IMethod>
{
    private readonly BoundTemplateMethod _boundTemplate;

    public OverrideFinalizerAdvice( AdviceConstructorParameters<IMethod> parameters, BoundTemplateMethod boundTemplate, IObjectReader tags )
        : base( parameters, tags )
    {
        this._boundTemplate = boundTemplate;
    }

    public override AdviceKind AdviceKind => AdviceKind.OverrideFinalizer;

    protected override OverrideMemberAdviceResult<IMethod> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        // TODO: order should be self if the target is introduced on the same layer.
        addTransformation( new OverrideFinalizerTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this._boundTemplate, this.Tags ) );

        return this.CreateSuccessResult();
    }
}