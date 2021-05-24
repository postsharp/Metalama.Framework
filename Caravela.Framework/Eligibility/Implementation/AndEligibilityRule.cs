// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Eligibility.Implementation
{
    internal class AndEligibilityRule<T> : IEligibilityRule<T>
    {
        private ImmutableArray<IEligibilityRule<T>> _rules;

        public AndEligibilityRule( ImmutableArray<IEligibilityRule<T>> rules )
        {
            this._rules = rules;
        }

        public EligibilityValue GetEligibility( T obj )
        {
            var eligibility = EligibilityValue.Eligible;

            foreach ( var predicate in this._rules )
            {
                var thisEligibility = predicate.GetEligibility( obj );

                if ( thisEligibility < eligibility )
                {
                    eligibility = thisEligibility;
                }
            }

            return eligibility;
        }

        public FormattableString? GetIneligibilityJustification(
            EligibilityValue requestedEligibility,
            IDescribedObject<T> describedObject )
        {
            foreach ( var predicate in this._rules )
            {
                var eligibility = predicate.GetEligibility( describedObject.Object );

                if ( eligibility < requestedEligibility )
                {
                    return predicate.GetIneligibilityJustification( requestedEligibility, describedObject );
                }
            }

            return null;
        }
    }
}