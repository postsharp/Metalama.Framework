// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.Aspects;
using Metalama.Framework.Impl.CodeModel.Builders;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.Advices
{
    internal class OverrideFieldOrPropertyAdvice : OverrideMemberAdvice<IFieldOrProperty>
    {
        public TemplateMember<IProperty> PropertyTemplate { get; }

        public TemplateMember<IMethod> GetTemplate { get; }

        public TemplateMember<IMethod> SetTemplate { get; }

        public OverrideFieldOrPropertyAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IFieldOrProperty targetDeclaration,
            TemplateMember<IProperty> propertyTemplate,
            TemplateMember<IMethod> getTemplate,
            TemplateMember<IMethod> setTemplate,
            string? layerName,
            Dictionary<string, object?>? tags )
            : base( aspect, templateInstance, targetDeclaration, layerName, tags )
        {
            this.PropertyTemplate = propertyTemplate;
            this.GetTemplate = getTemplate;
            this.SetTemplate = setTemplate;
        }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // TODO: Translate templates to this compilation.
            // TODO: order should be self if the target is introduced on the same layer.
            if ( this.TargetDeclaration is IField field )
            {
                var promotedField = new PromotedField( this, field );

                return AdviceResult.Create(
                    promotedField,
                    new OverriddenProperty( this, promotedField, this.PropertyTemplate, this.GetTemplate, this.SetTemplate ) );
            }
            else if ( this.TargetDeclaration is IProperty property )
            {
                return AdviceResult.Create( new OverriddenProperty( this, property, this.PropertyTemplate, this.GetTemplate, this.SetTemplate ) );
            }
            else
            {
                throw new AssertionFailedException();
            }
        }
    }
}