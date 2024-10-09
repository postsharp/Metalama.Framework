﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.Diagnostics;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceNamedTypeAdvice : IntroduceDeclarationAdvice<INamedType, NamedTypeBuilder>
{
    private readonly string _explicitName;

    public override AdviceKind AdviceKind => AdviceKind.IntroduceType;

    private OverrideStrategy OverrideStrategy { get; }

    public IntroduceNamedTypeAdvice(
        AdviceConstructorParameters<INamespaceOrNamedType> parameters,
        string explicitName,
        OverrideStrategy overrideStrategy,
        Action<NamedTypeBuilder>? buildAction )
        : base( parameters, buildAction )
    {
        this._explicitName = explicitName;
        this.OverrideStrategy = overrideStrategy;
    }

    protected override NamedTypeBuilder CreateBuilder( in AdviceImplementationContext context )
    {
        return new NamedTypeBuilder( this.AspectLayerInstance, (INamespaceOrNamedType) this.TargetDeclaration.AssertNotNull(), this._explicitName );
    }

    protected override IntroductionAdviceResult<INamedType> ImplementCore( NamedTypeBuilder builder, in AdviceImplementationContext context )
    {
        var targetDeclaration = (INamespaceOrNamedType) this.TargetDeclaration.ForCompilation( context.Compilation );

        var existingType =
            targetDeclaration switch
            {
                INamespace @namespace =>
                    @namespace.Types
                        .OfName( builder.Name )
                        .FirstOrDefault( t => builder.TypeParameters.Count == t.TypeParameters.Count ),
                INamedType namedType =>
                    namedType.AllTypes
                        .OfName( builder.Name )
                        .FirstOrDefault( t => builder.TypeParameters.Count == t.TypeParameters.Count ),
                _ => throw new AssertionFailedException( $"Unsupported: {targetDeclaration}" )
            };

        if ( existingType == null )
        {
            builder.Freeze();

            context.AddTransformation( builder.ToTransformation() );

            return this.CreateSuccessResult( AdviceOutcome.Default, builder );
        }
        else
        {
            switch ( this.OverrideStrategy )
            {
                case OverrideStrategy.Fail:
                    return this.CreateFailedResult(
                        AdviceDiagnosticDescriptors.CannotIntroduceNewTypeWhenItAlreadyExists.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, builder, targetDeclaration),
                            this ) );

                case OverrideStrategy.Ignore:
                    return this.CreateIgnoredResult( existingType );

                case OverrideStrategy.New:
                    builder.HasNewKeyword = builder.IsNew = true;
                    builder.Freeze();
                    context.AddTransformation( builder.ToTransformation() );

                    return this.CreateSuccessResult( AdviceOutcome.Default, builder );

                default:
                    throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
            }
        }
    }
}