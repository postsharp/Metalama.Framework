// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A base aspect that overrides the implementation of a method.
    /// </summary>
    /// <seealso href="@overriding-methods"/>
    [AttributeUsage( AttributeTargets.Method )]
    public abstract class OverrideMethodAspect : Attribute, IAspect<IMethod>
    {
        /// <inheritdoc />
        public virtual void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.AdviceFactory.OverrideMethod( builder.Target, nameof(this.OverrideMethod) );
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IMethod> builder )
        {
            builder.ExceptForInheritance().MustBeNonAbstract();
        }

        public virtual void BuildAspectClass( IAspectClassBuilder builder ) { }

        /// <summary>
        /// Default template of the new method implementation.
        /// </summary>
        /// <returns></returns>
        [Template]
        public abstract dynamic? OverrideMethod();
    }
}