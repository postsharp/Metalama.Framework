// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

#pragma warning disable 618 // Not implemented.

namespace Caravela.Framework.Eligibility.Implementation
{
    internal class EligibilityRule<T> : IEligibilityRule<T>
    {
        private readonly EligibleScenarios _ineligibility;
        private readonly Predicate<T> _predicate;
        private readonly Func<IDescribedObject<T>, FormattableString> _getJustification;

        public EligibilityRule( EligibleScenarios ineligibility, Predicate<T> predicate, Func<IDescribedObject<T>, FormattableString> getJustification )
        {
            this._ineligibility = ineligibility;
            this._predicate = predicate;
            this._getJustification = getJustification;
        }

        public EligibleScenarios GetEligibility( T obj ) => this._predicate( obj ) ? EligibleScenarios.All : this._ineligibility;

        public FormattableString? GetIneligibilityJustification( EligibleScenarios requestedEligibility, IDescribedObject<T> describedObject )
            => this._getJustification( describedObject );
    }
}