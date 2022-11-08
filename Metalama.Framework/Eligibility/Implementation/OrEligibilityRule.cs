// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;
using System;
using System.Collections.Immutable;
using System.Text;

namespace Metalama.Framework.Eligibility.Implementation
{
    internal class OrEligibilityRule<T> : IEligibilityRule<T>
        where T : class
    {
        private readonly ImmutableArray<IEligibilityRule<T>> _predicates;

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

                    stringBuilder.Append( justification.ToString( MetalamaExecutionContext.Current.FormatProvider ) );
                }
            }

            stringBuilder.Append( " }" );

            return $"{stringBuilder}";
        }
    }
}