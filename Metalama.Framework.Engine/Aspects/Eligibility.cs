// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Engine.Aspects
{
    public readonly struct Eligibility
    {
        public bool IsEligible { get; }

        public EligibleScenarios EligibleScenarios { get; }

        public string? Reason { get; }

        internal Eligibility( bool isEligible, EligibleScenarios eligibleScenarios, string? reason )
        {
            this.IsEligible = isEligible;
            this.EligibleScenarios = eligibleScenarios;
            this.Reason = reason;
        }
    }
}