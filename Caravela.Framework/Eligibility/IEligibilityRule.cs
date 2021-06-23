// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Eligibility
{
    /// <summary>
    /// Encapsulates a predicate determining the eligibility of an object (typically a declaration or a type) for an
    /// object extension (typically an aspect or aspect marker). (Not implemented.)
    /// </summary>
    /// <typeparam name="T">The type of object that the extension can be applied to.</typeparam>
    [Obsolete( "Not implemented." )]
    [CompileTimeOnly]
    public interface IEligibilityRule<in T>
    {
        /// <summary>
        /// Determines the eligibility of a given object for the current object extension.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        EligibilityValue GetEligibility( T obj );

        /// <summary>
        /// Gets the justification for which the <see cref="GetEligibility"/> method returned anything else than <see cref="EligibilityValue.Eligible"/>. 
        /// </summary>
        /// <param name="requestedEligibility">The eligibility that was requested by the user, but denied.</param>
        /// <param name="describedObject">The object for which the eligibility was denied, plus its description.</param>
        /// <returns></returns>
        FormattableString? GetIneligibilityJustification( EligibilityValue requestedEligibility, IDescribedObject<T> describedObject );
    }
}