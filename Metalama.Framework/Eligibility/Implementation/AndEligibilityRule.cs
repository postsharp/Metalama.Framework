// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Eligibility.Implementation
{
    internal class AndEligibilityRule<T> : IEligibilityRule<T>
    {
        private readonly ImmutableArray<IEligibilityRule<T>> _rules;

        public AndEligibilityRule( ImmutableArray<IEligibilityRule<T>> rules )
        {
            this._rules = rules;
        }

        public EligibleScenarios GetEligibility( T obj )
        {
            var eligibility = EligibleScenarios.All;

            foreach ( var predicate in this._rules )
            {
                eligibility &= predicate.GetEligibility( obj );

                if ( eligibility == EligibleScenarios.None )
                {
                    return EligibleScenarios.None;
                }
            }

            return eligibility;
        }

        public FormattableString? GetIneligibilityJustification(
            EligibleScenarios requestedEligibility,
            IDescribedObject<T> describedObject )
        {
            foreach ( var predicate in this._rules )
            {
                var eligibility = predicate.GetEligibility( describedObject.Object );

                if ( (eligibility & requestedEligibility) != requestedEligibility )
                {
                    return predicate.GetIneligibilityJustification( requestedEligibility, describedObject );
                }
            }

            return null;
        }
    }
}