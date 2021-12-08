// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.Diagnostics;

namespace Metalama.Framework.Engine.Aspects
{
    internal static class EligibilityExtensions
    {
        public static bool IncludesAll( this EligibleScenarios scenarios, EligibleScenarios subset ) => (scenarios & subset) == subset;

        public static bool IncludesAny( this EligibleScenarios scenarios, EligibleScenarios subset ) => (scenarios & subset) != 0;

        internal static Eligibility GetEligibility<T>(
            this IEligibilityRule<T> rule,
            T obj,
            EligibleScenarios requiredEligibility,
            bool requiresJustification = true )
            where T : class
        {
            var eligibility = rule.GetEligibility( obj );
            string? justification = null;

            if ( !eligibility.IncludesAll( requiredEligibility ) )
            {
                if ( requiresJustification )
                {
                    var describedObject = new DescribedObject<T>( obj );
                    justification = rule.GetIneligibilityJustification( requiredEligibility, describedObject )?.ToString( UserMessageFormatter.Instance );
                }

                return new Eligibility( false, eligibility, justification );
            }
            else
            {
                return new Eligibility( true, eligibility, null );
            }
        }
    }
}