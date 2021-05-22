namespace Caravela.Framework.Eligibility.Implementation
{
    internal class InheritanceOnlyEligibilityBuilder<T> : IEligibilityBuilder<T>
    {
        private readonly IEligibilityBuilder<T> _inner;

        public InheritanceOnlyEligibilityBuilder( IEligibilityBuilder<T> inner ) 
        {
            this._inner = inner;
        }

        public EligibilityValue Ineligibility => EligibilityValue.EligibleForInheritanceOnly;

        public void AddRule( IEligibilityRule<T> rule ) => this._inner.AddRule( rule );

        public IEligibilityRule<object> Build() => this._inner.Build();
    }
}