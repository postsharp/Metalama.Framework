// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Eligibility.Implementation
{
#pragma warning disable 618 // Not implemented.
    
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