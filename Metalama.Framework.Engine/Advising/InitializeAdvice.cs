// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    internal class InitializeAdvice : Advice
    {
        public BoundTemplateMethod BoundTemplate { get; }

        public InitializerKind Kind { get; }

        public new Ref<IMemberOrNamedType> TargetDeclaration => base.TargetDeclaration.As<IMemberOrNamedType>();

        public IObjectReader Tags { get; }

        public InitializeAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IMemberOrNamedType targetDeclaration,
            ICompilation sourceCompilation,
            BoundTemplateMethod boundTemplate,
            InitializerKind kind,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName )
        {
            this.BoundTemplate = boundTemplate;
            this.Kind = kind;
            this.Tags = tags;
        }

        public override AdviceImplementationResult Implement(
            IServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var containingType = targetDeclaration.GetDeclaringType().AssertNotNull();

            var constructors =
                targetDeclaration switch
                {
                    IConstructor constructor => new[] { constructor },
                    INamedType => this.Kind switch
                    {
                        InitializerKind.BeforeTypeConstructor =>
                            new[] { containingType.StaticConstructor },
                        InitializerKind.BeforeInstanceConstructor =>
                            containingType.Constructors
                                .Where( c => c.InitializerKind != ConstructorInitializerKind.This ),
                        _ => throw new AssertionFailedException()
                    },
                    _ => throw new AssertionFailedException()
                };

            foreach ( var ctor in constructors )
            {
                IConstructor targetCtor;

                if ( ctor.IsImplicitStaticConstructor() )
                {
                    // Missing static ctor.
                    var builder = new ConstructorBuilder( this, ctor.DeclaringType, this.Tags ) { IsStatic = true };
                    addTransformation( builder );
                    targetCtor = builder;
                }
                else if ( ctor.IsImplicitInstanceConstructor() )
                {
                    // Missing implicit ctor.
                    var builder = new ConstructorBuilder( this, ctor.DeclaringType, this.Tags );
                    addTransformation( builder );
                    targetCtor = builder;
                }
                else
                {
                    targetCtor = ctor;
                }

                var initialization = new TemplateBasedInitializationTransformation(
                    this,
                    targetDeclaration,
                    targetCtor,
                    this.BoundTemplate,
                    this.Tags );

                addTransformation( initialization );
            }

            return AdviceImplementationResult.Success();
        }
    }
}