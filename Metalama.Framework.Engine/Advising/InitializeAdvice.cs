// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Advising;

internal abstract class InitializeAdvice : Advice
{
    private readonly InitializerKind _kind;

    private new Ref<IMemberOrNamedType> TargetDeclaration => base.TargetDeclaration.As<IMemberOrNamedType>();

    protected InitializeAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance templateInstance,
        IMemberOrNamedType targetDeclaration,
        ICompilation sourceCompilation,
        InitializerKind kind,
        string? layerName ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName )
    {
        this._kind = kind;
    }

    public override AdviceImplementationResult Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        var containingType = targetDeclaration.GetClosestNamedType().AssertNotNull();

        if ( containingType.TypeKind is TypeKind.RecordClass or TypeKind.RecordStruct )
        {
            return AdviceImplementationResult.Failed(
                AdviceDiagnosticDescriptors.CannotAddInitializerToRecord.CreateRoslynDiagnostic(
                    containingType.GetDiagnosticLocation(),
                    (this.Aspect.AspectClass.ShortName, containingType) ) );
        }

        IConstructor? staticConstructor;

        if ( this._kind == InitializerKind.BeforeTypeConstructor )
        {
            staticConstructor = containingType.StaticConstructor;

            if ( staticConstructor == null || staticConstructor.IsImplicitlyDeclared )
            {
                var staticConstructorBuilder = new ConstructorBuilder( containingType, this ) { IsStatic = true };
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
                // Missing implicit ctor.
                var builder = new ConstructorBuilder( ctor.DeclaringType, this );
                addTransformation( builder.ToTransformation() );
                targetCtor = builder;
            }
            else
            {
                targetCtor = ctor;
            }

            this.AddTransformation( targetDeclaration, targetCtor, addTransformation );
        }

        return AdviceImplementationResult.Success();
    }

    protected abstract void AddTransformation( IMemberOrNamedType targetDeclaration, IConstructor targetCtor, Action<ITransformation> addTransformation );

    public override AdviceKind AdviceKind => AdviceKind.AddInitializer;
}