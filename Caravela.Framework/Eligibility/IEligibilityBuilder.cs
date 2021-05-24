// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Validation;
using System;

namespace Caravela.Framework.Eligibility
{
    [InternalImplement]
    public interface IEligibilityBuilder
    {
        /// <summary>
        /// Gets the <see cref="EligibilityValue"/> value that rules must return in case they evaluate negatively. The default
        /// value of this property is <see cref="EligibilityValue.Ineligible"/>, but it can be changed to <see cref="EligibilityValue.EligibleForInheritanceOnly"/>
        /// using <see cref="EligibilityExtensions.ExceptForInheritance{T}"/>.
        /// </summary>
        [Obsolete( "Not implemented." )]
        EligibilityValue Ineligibility { get; }

        /// <summary>
        /// Builds an immutable rule from the current builder instance.
        /// </summary>
        /// <returns></returns>
        [Obsolete( "Not implemented." )]
        IEligibilityRule<object> Build();
    }

    public interface IEligibilityBuilder<out T> : IEligibilityBuilder
    {
        /// <summary>
        /// Adds a rule to the current builder.
        /// </summary>
        /// <param name="rule"></param>
        [Obsolete( "Not implemented." )]
        void AddRule( IEligibilityRule<T> rule );
    }
}