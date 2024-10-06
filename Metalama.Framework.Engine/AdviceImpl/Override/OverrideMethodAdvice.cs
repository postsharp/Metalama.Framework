// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

internal sealed class OverrideMethodAdvice : OverrideMemberAdvice<IMethod, IMethod>
{
    private readonly BoundTemplateMethod _boundTemplate;

    public OverrideMethodAdvice( AdviceConstructorParameters<IMethod> parameters, BoundTemplateMethod boundTemplate, IObjectReader tags )
        : base( parameters, tags )
    {
        this._boundTemplate = boundTemplate;
    }

    public override AdviceKind AdviceKind => AdviceKind.OverrideMethod;

    protected override OverrideMemberAdviceResult<IMethod> Implement( in AdviceImplementationContext context )
    {
        // TODO: order should be self if the target is introduced on the same layer.
        var targetMethod = this.TargetDeclaration;

        switch ( targetMethod.MethodKind )
        {
            case MethodKind.Finalizer:
                context.AddTransformation( new OverrideFinalizerTransformation( this, this.TargetDeclaration.ToRef(), this._boundTemplate, this.Tags ) );

                break;

            case MethodKind.Operator:
                context.AddTransformation( new OverrideOperatorTransformation( this, this.TargetDeclaration.ToRef(), this._boundTemplate, this.Tags ) );

                break;

            default:
                context.AddTransformation( new OverrideMethodTransformation( this, this.TargetDeclaration.ToRef(), this._boundTemplate, this.Tags ) );

                break;
        }

        return this.CreateSuccessResult( targetMethod );
    }
}