// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Advices
{
    internal class InitializeAdvice : Advice
    {
        public TemplateMember<IMethod> Template { get; }

        public InitializerKind Kind { get; }

        public new Ref<IMemberOrNamedType> TargetDeclaration => base.TargetDeclaration.As<IMemberOrNamedType>();

        public InitializeAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IMemberOrNamedType targetDeclaration,
            TemplateMember<IMethod> template,
            InitializerKind kind,
            string? layerName,
            Dictionary<string, object?>? tags ) : base( aspect, templateInstance, targetDeclaration, layerName, tags )
        {
            this.Template = template;
            this.Kind = kind;
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var containingType =
                targetDeclaration switch
                {
                    INamedType t => t,
                    IMember m => m.DeclaringType,
                    _ => throw new AssertionFailedException()
                };

            // TODO: merging localConstructors with constructors from the compilation does not take into account signatures, only implicit instance ctor and missing static ctor.
            var localConstructors =
                observableTransformations
                    .OfType<IConstructorBuilder>()
                    .Where(
                        c => this.Kind switch
                        {
                            InitializerKind.BeforeTypeConstructor => c.IsStatic,
                            InitializerKind.BeforeInstanceConstructor => !c.IsStatic,
                            _ => throw new AssertionFailedException()
                        } )
                    .ToReadOnlyList();

            var constructors =
                this.Kind switch
                {
                    InitializerKind.BeforeTypeConstructor =>
                        localConstructors.Count == 0
                            ? new[] { containingType.StaticConstructor }
                            : localConstructors,
                    InitializerKind.BeforeInstanceConstructor =>
                        containingType.Constructors
                            .Where( c => c.InitializerKind != ConstructorInitializerKind.This )
                            .Where( c => !(c.Parameters.Count == 0 && localConstructors.Any( cc => cc.Parameters.Count == 0 )) )
                            .Concat( localConstructors ),
                    _ => throw new AssertionFailedException()
                };

            var transformations = new List<ITransformation>();

            foreach ( var ctor in constructors )
            {
                IConstructor targetCtor;

                if ( ctor.IsImplicitStaticConstructor() )
                {
                    // Missing static ctor.
                    var builder = new ConstructorBuilder( this, ctor.DeclaringType ) { IsStatic = true };
                    transformations.Add( builder );
                    targetCtor = builder;
                }
                else if ( ctor.IsImplicitInstanceConstructor() )
                {
                    // Missing implicit ctor.
                    var builder = new ConstructorBuilder( this, ctor.DeclaringType );
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
                    this.Template );

                transformations.Add( initialization );
            }

            return AdviceResult.Create( transformations );
        }
    }
}