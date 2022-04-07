// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Advices
{
    internal class InitializeAdvice : Advice
    {
        public TemplateMember<IMethod> Template { get; }

        public InitializationReason Reason { get; }

        public new IMemberOrNamedType TargetDeclaration => (IMemberOrNamedType) base.TargetDeclaration;

        public InitializeAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IMemberOrNamedType targetDeclaration,
            TemplateMember<IMethod> template,
            InitializationReason reason,
            string? layerName,
            Dictionary<string, object?>? tags ) : base( aspect, templateInstance, targetDeclaration, layerName, tags )
        {
            this.Template = template;
            this.Reason = reason;
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            // TODO: Everything.
        }

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
        {
            var containingType =
                this.TargetDeclaration switch
                {
                    INamedType t => t,
                    IMember m => m.DeclaringType,
                    _ => throw new AssertionFailedException()
                };

            // TODO: merging localConstructors with constructors from the compilation does not take into account signatures, only implicit instance ctor and missing static ctor.
            var localConstructors =
                observableTransformations
                    .OfType<IConstructorBuilder>()
                    .Where( c =>
                        c.IsStatic == ((this.Reason & InitializationReason.TypeConstructing) != 0)
                        || !c.IsStatic == ((this.Reason & InitializationReason.Constructing) != 0) )
                    .ToReadOnlyList();

            var constructors =
                ( (this.Reason & InitializationReason.TypeConstructing) != 0
                    ? new[] { containingType.StaticConstructor }
                    : Array.Empty<IConstructor>() )
                .Concat(
                    containingType.Constructors
                    .Where( c =>
                        !c.IsStatic == ((this.Reason & InitializationReason.Constructing) != 0)
                        && c.InitializerKind != ConstructorInitializerKind.This ) )
                .Where( c => !(c.Parameters.Count == 0 && localConstructors.Any( cc => cc.Parameters.Count == 0 && c.IsStatic == cc.IsStatic )) )
                .Concat( localConstructors );

            var transformations = new List<ITransformation>();

            foreach ( var ctor in constructors )
            {
                IConstructor targetCtor;

                if ( ctor.IsStatic && ctor.GetSymbol() == null && ctor is not IConstructorBuilder )
                {
                    // Missing static ctor.
                    var builder = new ConstructorBuilder( this, ctor.DeclaringType ) { IsStatic = true };
                    transformations.Add( builder );
                    targetCtor = builder;
                }
                else if ( !ctor.IsStatic && ctor.GetSymbol() != null && ctor.GetSymbol()!.GetPrimaryDeclaration() == null )
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
                    this.TargetDeclaration,
                    targetCtor,
                    this.Template,
                    this.Reason );

                transformations.Add( initialization );
            }

            return AdviceResult.Create( transformations );
        }
    }
}