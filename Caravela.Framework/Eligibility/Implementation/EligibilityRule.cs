using System;

namespace Caravela.Framework.Eligibility
{
    internal class EligibilityRule<T> : IEligibilityRule<T>
    {
        private readonly EligibilityValue _ineligibility;
        private readonly Predicate<T> _predicate;
        private readonly Func<IDescribedObject<T>, FormattableString> _getJustification;

        public EligibilityRule( EligibilityValue ineligibility, Predicate<T> predicate, Func<IDescribedObject<T>, FormattableString> getJustification )
        {
            this._ineligibility = ineligibility;
            this._predicate = predicate;
            this._getJustification = getJustification;
        }

        public EligibilityValue GetEligibility( T obj ) => this._predicate( obj ) ? EligibilityValue.Eligible : this._ineligibility;

        public FormattableString? GetIneligibilityJustification( EligibilityValue requestedEligibility, IDescribedObject<T> describedObject )
            => this._getJustification( describedObject );
    }
}