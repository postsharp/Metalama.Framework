// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

internal sealed class OverrideMethodAdvice : OverrideMemberAdvice<IMethod, IMethod>
{
    private readonly BoundTemplateMethod _boundTemplate;

    public OverrideMethodAdvice(
        IAspectInstanceInternal aspectInstance,
        TemplateClassInstance templateInstance,
        IMethod targetDeclaration,
        ICompilation sourceCompilation,
        BoundTemplateMethod boundTemplate,
        string? layerName,
        IObjectReader tags ) : base( aspectInstance, templateInstance, targetDeclaration, sourceCompilation, layerName, tags )
    {
        this._boundTemplate = boundTemplate;
    }

    public override AdviceKind AdviceKind => AdviceKind.OverrideMethod;

    protected override OverrideMemberAdviceResult<IMethod> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        // TODO: order should be self if the target is introduced on the same layer.
        var targetMethod = this.TargetDeclaration.GetTarget( compilation );

        switch ( targetMethod.MethodKind )
        {
            case MethodKind.Finalizer:
                addTransformation(
                    new OverrideFinalizerTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this._boundTemplate, this.Tags ) );

                break;

            case MethodKind.Operator:
                addTransformation(
                    new OverrideOperatorTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this._boundTemplate, this.Tags ) );

                break;

            default:
                addTransformation(
                    new OverrideMethodTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this._boundTemplate, this.Tags ) );

                break;
        }

        return this.CreateSuccessResult( targetMethod );
    }
}