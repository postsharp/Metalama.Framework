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
    internal class OverrideFieldOrPropertyAdvice : OverrideMemberAdvice<IFieldOrProperty>
    {
        public TemplateMember<IProperty> PropertyTemplate { get; }

        public BoundTemplateMethod GetTemplate { get; }

        public BoundTemplateMethod SetTemplate { get; }

        public OverrideFieldOrPropertyAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IFieldOrProperty targetDeclaration,
            ICompilation sourceCompilation,
            TemplateMember<IProperty> propertyTemplate,
            BoundTemplateMethod getTemplate,
            BoundTemplateMethod setTemplate,
            string? layerName,
            IObjectReader tags )
            : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName, tags )
        {
            this.PropertyTemplate = propertyTemplate;
            this.GetTemplate = getTemplate;
            this.SetTemplate = setTemplate;
        }

        public override AdviceImplementationResult Implement(
            IServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            // TODO: Translate templates to this compilation.
            // TODO: order should be self if the target is introduced on the same layer.
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var promotedField = OverrideHelper.OverrideProperty(
                serviceProvider,
                this,
                targetDeclaration,
                this.GetTemplate,
                this.SetTemplate,
                this.Tags,
                addTransformation );

            return AdviceImplementationResult.Success( promotedField );
        }
    }
}