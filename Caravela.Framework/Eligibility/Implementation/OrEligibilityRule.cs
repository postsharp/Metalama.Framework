// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Immutable;
using System.Text;

namespace Caravela.Framework.Eligibility.Implementation
{
    internal class OrEligibilityRule<T> : IEligibilityRule<T>
    {
        private ImmutableArray<IEligibilityRule<T>> _predicates;

        public OrEligibilityRule( ImmutableArray<IEligibilityRule<T>> predicates )
        {
            this._predicates = predicates;
        }

        public EligibleScenarios GetEligibility( T obj )
        {
            var eligibility = EligibleScenarios.None;

            foreach ( var predicate in this._predicates )
            {
                eligibility |= predicate.GetEligibility( obj );

                if ( eligibility == EligibleScenarios.All )
                {
                    return EligibleScenarios.All;
                }
            }

            return eligibility;
        }

        public FormattableString? GetIneligibilityJustification(
            EligibleScenarios requestedEligibility,
            IDescribedObject<T> describedObject )
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append( "none of these conditions was fulfilled: { " );

            for ( var i = 0; i < this._predicates.Length; i++ )
            {
                var predicate = this._predicates[i];
                var justification = predicate.GetIneligibilityJustification( requestedEligibility, describedObject );

                if ( justification != null )
                {
                    if ( i > 0 )
                    {
                        stringBuilder.Append( " or " );
                    }

                    stringBuilder.Append( justification.ToString( CaravelaStaticServices.FormatProvider ) );
                }
            }

            stringBuilder.Append( " }" );

            return $"{stringBuilder}";
        }
    }
}