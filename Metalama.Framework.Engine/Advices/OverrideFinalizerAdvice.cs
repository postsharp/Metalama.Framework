// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;

namespace Metalama.Framework.Engine.Advices
{
    internal class OverrideFinalizerAdvice : OverrideMemberAdvice<IFinalizer>
    {
        public BoundTemplateMethod BoundTemplate { get; }

        public OverrideFinalizerAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IFinalizer targetDeclaration,
            BoundTemplateMethod boundTemplate,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, layerName, tags )
        {
            this.BoundTemplate = boundTemplate;
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            var target = this.TargetDeclaration.GetTarget( compilation );

            if ( target.IsImplicit )
            {
                var builder = new FinalizerBuilder( this, target.DeclaringType, this.Tags );

                return AdviceResult.Create(
                    builder,
                    new OverrideFinalizerTransformation( this, builder, this.BoundTemplate, this.Tags ) );
            }
            else
            {
                return AdviceResult.Create(
                    new OverrideFinalizerTransformation( this, target, this.BoundTemplate, this.Tags ) );
            }
        }
    }
}