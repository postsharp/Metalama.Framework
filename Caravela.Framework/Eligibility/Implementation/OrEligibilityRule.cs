// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Immutable;
using System.Text;

namespace Caravela.Framework.Eligibility.Implementation
{
#pragma warning disable 618 // Not implemented.
    
    internal class OrEligibilityRule<T> : IEligibilityRule<T>
    {
        private ImmutableArray<IEligibilityRule<T>> _predicates;

        public OrEligibilityRule( ImmutableArray<IEligibilityRule<T>> predicates )
        {
            this._predicates = predicates;
        }

        public EligibilityValue GetEligibility( T obj )
        {
            var eligibility = EligibilityValue.Ineligible;

            foreach ( var predicate in this._predicates )
            {
                var thisEligibility = predicate.GetEligibility( obj );

                if ( thisEligibility > eligibility )
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
                        if ( i == this._predicates.Length - 1 )
                        {
                            stringBuilder.Append( ", or " );
                        }
                        else
                        {
                            stringBuilder.Append( ", " );
                        }
                    }

                    stringBuilder.Append( justification.ToString( describedObject.FormatProvider ) );
                }
            }

            stringBuilder.Append( " }" );

            return $"{stringBuilder}";
        }
    }
}