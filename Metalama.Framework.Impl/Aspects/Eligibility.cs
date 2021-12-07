// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Eligibility;

namespace Caravela.Framework.Impl.Aspects
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