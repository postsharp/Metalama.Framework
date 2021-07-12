// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class OverrideFieldOrPropertyAdvice : Advice
    {
        public IProperty? TemplateProperty { get; }

        public IMethod? GetTemplateMethod { get; }

        public IMethod? SetTemplateMethod { get; }

        public new IFieldOrProperty TargetDeclaration => (IFieldOrProperty) base.TargetDeclaration;

        public OverrideFieldOrPropertyAdvice(
            AspectInstance aspect,
            IFieldOrProperty targetDeclaration,
            IProperty? templateProperty,
            IMethod? getTemplateMethod,
            IMethod? setTemplateMethod,
            string layerName,
            Dictionary<string, object?>? tags )
            : base( aspect, targetDeclaration, layerName, tags )
        {
            // We need either property template or (one or more) accessor templates, but never both.
            Invariant.Assert( templateProperty != null || getTemplateMethod != null || setTemplateMethod != null );
            Invariant.Assert( !(templateProperty != null && (getTemplateMethod != null || setTemplateMethod != null)) );

            this.TemplateProperty = templateProperty;
            this.GetTemplateMethod = getTemplateMethod;
            this.SetTemplateMethod = setTemplateMethod;
        }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // TODO: Translate templates to this compilation.
            if ( this.TargetDeclaration is IField field )
            {
                var promotedField = new PromotedField( this, field );

                return AdviceResult.Create(
                    promotedField,
                    new OverriddenProperty( this, promotedField, this.TemplateProperty, this.GetTemplateMethod, this.SetTemplateMethod ) );
            }
            else if ( this.TargetDeclaration is IProperty property )
            {
                return AdviceResult.Create(
                    new OverriddenProperty( this, property, this.TemplateProperty, this.GetTemplateMethod, this.SetTemplateMethod ) );
            }
            else
            {
                throw new AssertionFailedException();
            }
        }
    }
}