// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using System;

namespace Metalama.Framework.Engine.Advices
{
    internal class OverrideFieldOrPropertyAdvice : OverrideMemberAdvice<IFieldOrPropertyOrIndexer>
    {
        public TemplateMember<IProperty> PropertyTemplate { get; }

        public BoundTemplateMethod GetTemplate { get; }

        public BoundTemplateMethod SetTemplate { get; }

        public OverrideFieldOrPropertyAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IFieldOrPropertyOrIndexer targetDeclaration,
            TemplateMember<IProperty> propertyTemplate,
            BoundTemplateMethod getTemplate,
            BoundTemplateMethod setTemplate,
            string? layerName,
            IObjectReader tags )
            : base( aspect, templateInstance, targetDeclaration, layerName, tags )
        {
            this.PropertyTemplate = propertyTemplate;
            this.GetTemplate = getTemplate;
            this.SetTemplate = setTemplate;
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( IServiceProvider serviceProvider, ICompilation compilation )
        {
            // TODO: Translate templates to this compilation.
            // TODO: order should be self if the target is introduced on the same layer.
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            return AdviceResult.Create(
                OverrideHelper.OverrideProperty(
                    serviceProvider,
                    this,
                    targetDeclaration,
                    this.GetTemplate,
                    this.SetTemplate,
                    this.Tags ) );
        }
    }
}