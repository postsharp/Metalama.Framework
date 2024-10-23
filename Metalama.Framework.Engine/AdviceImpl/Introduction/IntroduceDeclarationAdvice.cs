﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.Diagnostics;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal abstract class IntroduceDeclarationAdvice<TIntroduced, TBuilder> : Advice<IntroductionAdviceResult<TIntroduced>>
    where TIntroduced : class, IDeclaration
    where TBuilder : DeclarationBuilder, TIntroduced
{
    private readonly Action<TBuilder>? _buildAction;

    protected IntroduceDeclarationAdvice( AdviceConstructorParameters parameters, Action<TBuilder>? buildAction )
        : base( parameters )
    {
        this._buildAction = buildAction;
    }

    protected IntroductionAdviceResult<TIntroduced> CreateSuccessResult( AdviceOutcome outcome, TIntroduced introducedMember )
    {
        return new IntroductionAdviceResult<TIntroduced>( this.AdviceKind, outcome, introducedMember.ToRef().As<TIntroduced>(), null );
    }

    protected IntroductionAdviceResult<TIntroduced> CreateIgnoredResult( IMemberOrNamedType existingMember )
        => new(
            this.AdviceKind,
            AdviceOutcome.Ignore,
            existingMember is TIntroduced typedMember ? typedMember.ToRef().As<TIntroduced>() : null,
            existingMember.ToRef() );

    protected sealed override IntroductionAdviceResult<TIntroduced> Implement( in AdviceImplementationContext context )
    {
        var builder = this.CreateBuilder( context );
        context.ThrowIfAnyError();

        this.InitializeBuilder( builder, in context );

        this._buildAction?.Invoke( builder );

        this.CompleteBuilder( builder, in context );

        this.ValidateBuilder( builder, context.Diagnostics );

        return this.ImplementCore( builder, in context );
    }

    protected abstract TBuilder CreateBuilder( in AdviceImplementationContext context );

    protected virtual void InitializeBuilder( TBuilder builder, in AdviceImplementationContext context ) { }

    protected virtual void CompleteBuilder( TBuilder builder, in AdviceImplementationContext context ) { }

    protected abstract IntroductionAdviceResult<TIntroduced> ImplementCore( TBuilder builder, in AdviceImplementationContext context );

    protected virtual void ValidateBuilder( TBuilder builder, IDiagnosticAdder diagnosticAdder ) { }

    public override string ToString() => $"Introduce {typeof(TIntroduced).Name}";
}