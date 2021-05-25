// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

#pragma warning disable 618

namespace Caravela.Framework.Eligibility.Implementation
{
    internal class CastEligibilityRule<TIn, TOut> : IEligibilityRule<TOut>
        where TIn : class
        where TOut : class
    {
        private readonly IEligibilityRule<TIn> _inner;

        public CastEligibilityRule( IEligibilityRule<TIn> inner )
        {
            this._inner = inner;
        }

        public EligibilityValue GetEligibility( TOut obj )
        {
            if ( obj is TIn convertedMember )
            {
                return this._inner.GetEligibility( convertedMember );
            }
            else
            {
                return EligibilityValue.Ineligible;
            }
        }

        public FormattableString? GetIneligibilityJustification(
            EligibilityValue requestedEligibility,
            IDescribedObject<TOut> describedObject )
        {
            if ( describedObject.Object is TIn )
            {
                return this._inner.GetIneligibilityJustification( requestedEligibility, describedObject.Cast<TOut, TIn>() );    
            }
            else
            {
                return $"{describedObject} is not an {typeof(TIn).Name}";
            }
        }
    }
}