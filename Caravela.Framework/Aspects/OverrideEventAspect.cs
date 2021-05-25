﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A base aspect that overrides the implementation of a method.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
    public abstract class OverrideEventAspect : Attribute, IAspect<IEvent>
    {
        /// <inheritdoc />
        public virtual void BuildAspect( IAspectBuilder<IEvent> builder )
        {
            builder.AdviceFactory.OverrideEventAccessors(
                builder.TargetDeclaration,
                nameof(this.OverrideAdd),
                nameof(this.OverrideRemove),
                nameof(this.OverrideInvoke) );
        }

        [OverrideEventAddTemplate]
        public abstract void OverrideAdd( dynamic handler );

        [OverrideEventRemoveTemplate]
        public abstract void OverrideRemove( dynamic handler );

        [OverrideEventInvokeTemplate]
        public abstract void OverrideInvoke( dynamic handler );

        public virtual void BuildEligibility( IEligibilityBuilder<IEvent> builder )
        {
            builder.ExceptForInheritance().MustBeNonAbstract();
        }
    }
}