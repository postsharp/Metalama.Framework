// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advices
{
    internal class OverrideFieldOrPropertyAdvice : OverrideMemberAdvice<IFieldOrPropertyOrIndexer>
    {
        public TemplateMember<IProperty> PropertyTemplate { get; }

        public TemplateMember<IMethod> GetTemplate { get; }

        public TemplateMember<IMethod> SetTemplate { get; }

        public OverrideFieldOrPropertyAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IFieldOrPropertyOrIndexer targetDeclaration,
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

        public override void Initialize( IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
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