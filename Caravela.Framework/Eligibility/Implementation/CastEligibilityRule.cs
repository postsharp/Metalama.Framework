using System;

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
            var convertedMember = obj as TIn;

            if ( convertedMember == null )
            {
                return EligibilityValue.Ineligible;
            }

            return this._inner.GetEligibility( convertedMember );
        }

        public FormattableString? GetIneligibilityJustification(
            EligibilityValue requestedEligibility,
            IDescribedObject<TOut> describedObject )
        {
            var convertedMember = describedObject.Object as TIn;

            if ( convertedMember == null )
            {
                return $"{describedObject} is not an {typeof(TIn).Name}";
            }

            return this._inner.GetIneligibilityJustification( requestedEligibility, describedObject.Cast<TOut,TIn>() );
        }
    }
}