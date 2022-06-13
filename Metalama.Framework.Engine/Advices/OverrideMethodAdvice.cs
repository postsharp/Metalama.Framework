// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advices
{
    internal class OverrideMethodAdvice : OverrideMemberAdvice<IMethod>
    {
        public BoundTemplateMethod BoundTemplate { get; }

        public OverrideMethodAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IMethod targetDeclaration,
            BoundTemplateMethod boundTemplate,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, layerName, tags )
        {
            this.BoundTemplate = boundTemplate;
        }

        public override AdviceImplementationResult Implement( IServiceProvider serviceProvider, ICompilation compilation )
        {
            // TODO: order should be self if the target is introduced on the same layer.
            return AdviceImplementationResult.Create(
                new OverrideMethodTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this.BoundTemplate, this.Tags ) );
        }
    }
}