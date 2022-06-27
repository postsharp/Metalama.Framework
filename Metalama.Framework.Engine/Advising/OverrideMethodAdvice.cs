// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising
{
    internal class OverrideMethodAdvice : OverrideMemberAdvice<IMethod>
    {
        public BoundTemplateMethod BoundTemplate { get; }

        public OverrideMethodAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IMethod targetDeclaration,
            ICompilation sourceCompilation,
            BoundTemplateMethod boundTemplate,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName, tags )
        {
            this.BoundTemplate = boundTemplate;
        }

        public override AdviceImplementationResult Implement(
            IServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            // TODO: order should be self if the target is introduced on the same layer.
            var targetMethod = this.TargetDeclaration.GetTarget( compilation );

            switch ( targetMethod.MethodKind )
            {
                case MethodKind.Finalizer:
                    addTransformation(
                        new OverrideFinalizerTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this.BoundTemplate, this.Tags ) );

                    break;

                case MethodKind.ConversionOperator:
                case MethodKind.UserDefinedOperator:
                    addTransformation(
                        new OverrideOperatorTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this.BoundTemplate, this.Tags ) );

                    break;

                default:
                    addTransformation(
                        new OverrideMethodTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this.BoundTemplate, this.Tags ) );

                    break;
            }

            return AdviceImplementationResult.Success();
        }
    }
}