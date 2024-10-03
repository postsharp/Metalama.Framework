﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.AdviceImpl.Initialization;

internal abstract class InitializeAdvice : Advice<AddInitializerAdviceResult>
{
    private readonly InitializerKind _kind;

    private new IRef<IMemberOrNamedType> TargetDeclaration => base.TargetDeclaration.As<IMemberOrNamedType>();

    protected InitializeAdvice( AdviceConstructorParameters<IMemberOrNamedType> parameters, InitializerKind kind ) : base( parameters )
    {
        this._kind = kind;
    }

    protected override AddInitializerAdviceResult Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        var containingType = targetDeclaration.GetClosestNamedType().AssertNotNull();

        if ( targetDeclaration is INamedType && containingType.TypeKind is TypeKind.RecordClass or TypeKind.RecordStruct )
        {
            return this.CreateFailedResult(
                AdviceDiagnosticDescriptors.CannotAddInitializerToRecord.CreateRoslynDiagnostic(
                    containingType.GetDiagnosticLocation(),
                    (this.AspectInstance.AspectClass.ShortName, containingType),
                    this ) );
        }

        IConstructor? staticConstructor;

        if ( this._kind == InitializerKind.BeforeTypeConstructor )
        {
            staticConstructor = containingType.StaticConstructor;

            if ( staticConstructor == null || staticConstructor.IsImplicitlyDeclared )
            {
                var staticConstructorBuilder =
                    new ConstructorBuilder( this, containingType ) { IsStatic = true, ReplacedImplicitConstructor = staticConstructor };

                staticConstructor = staticConstructorBuilder;
                addTransformation( staticConstructorBuilder.ToTransformation() );
            }
        }
        else
        {
            staticConstructor = null;
        }

        var constructors =
            targetDeclaration switch
            {
                IConstructor constructor => new[] { constructor },
                INamedType => this._kind switch
                {
                    InitializerKind.BeforeTypeConstructor =>
                        new[] { staticConstructor.AssertNotNull() },
                    InitializerKind.BeforeInstanceConstructor =>
                        containingType.Constructors
                            .Where( c => c.InitializerKind != ConstructorInitializerKind.This ),
                    _ => throw new AssertionFailedException( $"Unexpected initializer kind: {this._kind}." )
                },
                _ => throw new AssertionFailedException( $"Unexpected declaration: '{targetDeclaration}'." )
            };

        foreach ( var ctor in constructors )
        {
            IConstructor targetCtor;

            if ( ctor.IsImplicitInstanceConstructor() )
            {
                // Missing explicit ctor.
                var builder =
                    new ConstructorBuilder( this, ctor.DeclaringType ) { ReplacedImplicitConstructor = ctor, Accessibility = Accessibility.Public };

                addTransformation( builder.ToTransformation() );
                targetCtor = builder;
            }
            else
            {
                targetCtor = ctor;
            }

            this.AddTransformation( targetDeclaration, targetCtor, addTransformation );
        }

        return new AddInitializerAdviceResult { AdviceKind = this.AdviceKind };
    }

    protected abstract void AddTransformation( IMemberOrNamedType targetDeclaration, IConstructor targetCtor, Action<ITransformation> addTransformation );

    public override AdviceKind AdviceKind => AdviceKind.AddInitializer;
}