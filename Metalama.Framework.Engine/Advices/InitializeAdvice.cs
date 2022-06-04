// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Advices
{
    internal class InitializeAdvice : Advice
    {
        public BoundTemplateMethod BoundTemplate { get; }

        public InitializerKind Kind { get; }

        public new Ref<IMemberOrNamedType> TargetDeclaration => base.TargetDeclaration.As<IMemberOrNamedType>();

        public InitializeAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IMemberOrNamedType targetDeclaration,
            BoundTemplateMethod boundTemplate,
            InitializerKind kind,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, layerName, tags )
        {
            this.BoundTemplate = boundTemplate;
            this.Kind = kind;
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation )
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
                

            var transformations = new List<ITransformation>();

            foreach ( var ctor in constructors )
            {
                IConstructor targetCtor;

                if ( ctor.IsImplicitStaticConstructor() )
                {
                    // Missing static ctor.
                    var builder = new ConstructorBuilder( this, ctor.DeclaringType, this.Tags ) { IsStatic = true };
                    transformations.Add( builder );
                    targetCtor = builder;
                }
                else if ( ctor.IsImplicitInstanceConstructor() )
                {
                    // Missing implicit ctor.
                    var builder = new ConstructorBuilder( this, ctor.DeclaringType, this.Tags );
                    transformations.Add( builder );
                    targetCtor = builder;
                }
                else
                {
                    targetCtor = ctor;
                }

                var initialization = new InitializationTransformation(
                    this,
                    targetDeclaration,
                    targetCtor,
                    this.BoundTemplate,
                    this.Tags );

                transformations.Add( initialization );
            }

            return AdviceResult.Create( transformations );
        }
    }
}