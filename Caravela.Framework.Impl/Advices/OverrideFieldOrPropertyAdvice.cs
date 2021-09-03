// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class OverrideFieldOrPropertyAdvice : OverrideMemberAdvice<IFieldOrProperty>
    {
        public Template<IProperty> PropertyTemplate { get; }

        public Template<IMethod> GetTemplate { get; }

        public Template<IMethod> SetTemplate { get; }

        public OverrideFieldOrPropertyAdvice(
            AspectInstance aspect,
            IFieldOrProperty targetDeclaration,
            Template<IProperty> propertyTemplate,
            Template<IMethod> getTemplate,
            Template<IMethod> setTemplate,
            string? layerName,
            Dictionary<string, object?>? tags )
            : base( aspect, targetDeclaration, layerName, tags )
        {
            // We need either property template or (one or more) accessor templates, but never both.
            Invariant.Assert( !propertyTemplate.IsNull || !getTemplate.IsNull || !setTemplate.IsNull );
            Invariant.Assert( !(!propertyTemplate.IsNull && (!getTemplate.IsNull || !setTemplate.IsNull)) );

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