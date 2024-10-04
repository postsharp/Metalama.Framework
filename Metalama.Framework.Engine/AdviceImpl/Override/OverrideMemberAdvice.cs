// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

internal abstract class OverrideMemberAdvice<TInput, TOutput> : Advice<OverrideMemberAdviceResult<TOutput>>
    where TInput : class, IMember
    where TOutput : class, IMember
{
    protected new IRef<TInput> TargetDeclaration => base.TargetDeclaration.As<TInput>();

    protected IObjectReader Tags { get; }

    protected OverrideMemberAdvice( AdviceConstructorParameters<TInput> parameters, IObjectReader tags ) : base( parameters )
    {
        this.Tags = tags;
    }

    public override string ToString() => $"Override {this.TargetDeclaration}";

    protected OverrideMemberAdviceResult<TOutput> CreateSuccessResult( TOutput? member = null )
        => new( member?.ToRef().As<TOutput>() ) { AdviceKind = this.AdviceKind, Outcome = AdviceOutcome.Default };
}