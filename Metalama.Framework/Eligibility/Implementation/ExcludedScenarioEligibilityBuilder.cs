// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Eligibility.Implementation
{
    internal class ExcludedScenarioEligibilityBuilder<T> : IEligibilityBuilder<T>
    {
        private readonly IEligibilityBuilder<T> _inner;

        public ExcludedScenarioEligibilityBuilder( IEligibilityBuilder<T> inner, EligibleScenarios excludedScenario )
        {
            this._inner = inner;
            this.IneligibleScenarios = excludedScenario;
        }

        public EligibleScenarios IneligibleScenarios { get; }

        public void AddRule( IEligibilityRule<T> rule ) => this._inner.AddRule( rule );

        public IEligibilityRule<IDeclaration> Build() => this._inner.Build();
    }
}