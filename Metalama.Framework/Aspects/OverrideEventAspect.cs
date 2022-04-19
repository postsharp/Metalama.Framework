// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// A base aspect that overrides the implementation of an event.
    /// </summary>
    /// <seealso href="@overriding-events"/>
    [AttributeUsage( AttributeTargets.Event )]
    public abstract class OverrideEventAspect : EventAspect
    {
        /// <inheritdoc />
        public override void BuildAspect( IAspectBuilder<IEvent> builder )
        {
            builder.Advices.OverrideAccessors(
                builder.Target,
                nameof(this.OverrideAdd),
                nameof(this.OverrideRemove),
                null );
        }

        // TODO: When template parameters are properly resolved during expansion, the parameter name here should change to "handler".
        [Template]
        public abstract void OverrideAdd( dynamic value );

        [Template]
        public abstract void OverrideRemove( dynamic value );

        // TODO: Add this back after invoke overrides are implemented.
        // [Template]
        // public abstract void OverrideInvoke( dynamic handler );

        public override void BuildEligibility( IEligibilityBuilder<IEvent> builder )
        {
            builder.ExceptForInheritance().MustBeNonAbstract();
        }
    }
}