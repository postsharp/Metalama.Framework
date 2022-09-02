// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Eligibility.Implementation;
using System;

namespace Metalama.Framework.Eligibility;

/// <summary>
/// Creates instances of the <see cref="IEligibilityRule{T}"/> interface, which can then be used by the <see cref="IAspectBuilder{TAspectTarget}.VerifyEligibility"/> method.
/// </summary>
[CompileTime]
public static class EligibilityRuleFactory
{
    /// <summary>
    /// Create an instance of the <see cref="IEligibilityRule{T}"/> interface, which can then be used by the <see cref="IAspectBuilder{TAspectTarget}.VerifyEligibility"/> method.
    /// </summary>
    /// <remarks>
    /// Eligibility rules are heavy and expensive objects although their evaluation is fast and efficient. It is recommended to store rules in static fields of the aspect. 
    /// </remarks>
    public static IEligibilityRule<T> CreateRule<T>( Action<IEligibilityBuilder<T>> predicate, params Action<IEligibilityBuilder<T>>[]? otherPredicates )
        where T : class
    {
        var builder = new EligibilityBuilder<T>();
        predicate( builder );

        if ( otherPredicates != null )
        {
            foreach ( var otherPredicate in otherPredicates )
            {
                otherPredicate( builder );
            }
        }

        return builder.Build();
    }
}