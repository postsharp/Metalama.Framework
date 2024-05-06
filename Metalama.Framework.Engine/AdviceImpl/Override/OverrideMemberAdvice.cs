// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

internal abstract class OverrideMemberAdvice<TInput, TOutput> : Advice<OverrideMemberAdviceResult<TOutput>>
    where TInput : class, IMember
    where TOutput : class, IMember
{
    protected new Ref<TInput> TargetDeclaration => base.TargetDeclaration.As<TInput>();

    protected IObjectReader Tags { get; }

    protected OverrideMemberAdvice(
        IAspectInstanceInternal aspectInstance,
        TemplateClassInstance templateInstance,
        TInput targetDeclaration,
        ICompilation sourceCompilation,
        string? layerName,
        IObjectReader tags ) : base( aspectInstance, templateInstance, targetDeclaration, sourceCompilation, layerName )
    {
        this.Tags = tags;
    }

    public override string ToString() => $"Override {this.TargetDeclaration}";

    protected OverrideMemberAdviceResult<TOutput> CreateSuccessResult( TOutput? member = null )
        => new( member?.ToTypedRef() ) { AdviceKind = this.AdviceKind, Outcome = AdviceOutcome.Default };
}