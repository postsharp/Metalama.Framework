// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal abstract class IntroduceDeclarationAdvice<TIntroduced, TBuilder> : Advice<IntroductionAdviceResult<TIntroduced>>
    where TIntroduced : class, IDeclaration
    where TBuilder : DeclarationBuilder
{
    protected TBuilder Builder { get; init; }

    public Action<TBuilder>? BuildAction { get; }

    protected IntroduceDeclarationAdvice( AdviceConstructorParameters parameters, Action<TBuilder>? buildAction )
        : base( parameters )
    {
        this.BuildAction = buildAction;

        // This is to make the nullability analyzer happy. Derived classes are supposed to set this member in the
        // constructor. Other designs are more cumbersome.
        this.Builder = null!;
    }

    protected IntroductionAdviceResult<TIntroduced> CreateSuccessResult( AdviceOutcome outcome = AdviceOutcome.Default, TIntroduced? member = null )
    {
        var memberRef = member != null ? member.ToTypedRef().As<TIntroduced>() : ((TIntroduced) (IDeclaration) this.Builder).ToTypedRef();

        return new IntroductionAdviceResult<TIntroduced>( this.AdviceKind, outcome, memberRef, null );
    }

    protected IntroductionAdviceResult<TIntroduced> CreateIgnoredResult( IMember existingMember )
        => new(
            this.AdviceKind,
            AdviceOutcome.Ignore,
            existingMember is TIntroduced typedMember ? typedMember.ToTypedRef() : null,
            existingMember.ToTypedRef() );

    public override string ToString() => $"Introduce {this.Builder}";
}