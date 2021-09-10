// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A base aspect that overrides the implementation of a method.
    /// </summary>
    /// <seealso href="@overriding-fields-or-properties"/>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
    public abstract class OverrideFieldOrPropertyAspect : Attribute, IAspect<IFieldOrProperty>
    {
        /// <inheritdoc />
        public virtual void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            var getterTemplateSelector = new GetterTemplateSelector(
                "get_" + nameof(this.OverrideProperty),
                "get_" + nameof(this.OverrideEnumerableProperty),
                "get_" + nameof(this.OverrideEnumeratorProperty) );

            builder.AdviceFactory.OverrideFieldOrPropertyAccessors( builder.Target, getterTemplateSelector, "set_" + nameof(this.OverrideProperty) );
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IFieldOrProperty> builder ) => builder.ExceptForInheritance().MustBeNonAbstract();

        public virtual void BuildAspectClass( IAspectClassBuilder builder ) { }

        [Template]
        public abstract dynamic? OverrideProperty
        {
            get;
            set;
        }

        [Abstract]
        [Template]
        public virtual IEnumerable<dynamic?> OverrideEnumerableProperty => throw new NotSupportedException();

        [Abstract]
        [Template]
        public virtual IEnumerator<dynamic?> OverrideEnumeratorProperty => throw new NotSupportedException();
    }
}