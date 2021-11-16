// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Validation;

namespace Caravela.Framework.Eligibility
{
#error Deliberately causing a build issue.

    /// <summary>
    /// The non-generic base interface for <see cref="IEligibilityBuilder{T}"/>.
    /// </summary>
    /// <seealso href="@eligibility"/> 
    [InternalImplement]
    [CompileTimeOnly]
    public interface IEligibilityBuilder
    {
        /// <summary>
        /// Gets the <see cref="EligibleScenarios"/> value that rules must return in case they evaluate negatively. The default
        /// value of this property is <see cref="EligibleScenarios.None"/>, but it can be changed to anything
        /// using <see cref="EligibilityExtensions.ExceptForScenarios{T}"/>.
        /// </summary>
        EligibleScenarios IneligibleScenarios { get; }

        /// <summary>
        /// Builds an immutable rule from the current builder instance.
        /// </summary>
        /// <returns></returns>
        IEligibilityRule<IDeclaration> Build();
    }

    /// <summary>
    /// The argument of <see cref="IEligible{T}.BuildEligibility"/>. Allows the implementation to add requirements
    /// using methods of <see cref="EligibilityExtensions"/>.
    /// </summary>
    /// <typeparam name="T">Type of declaration.</typeparam>
    /// <seealso href="@eligibility"/>
    /// <seealso cref="EligibilityExtensions"/> 
    public interface IEligibilityBuilder<out T> : IEligibilityBuilder
    {
        /// <summary>
        /// Adds a rule to the current builder. For convenience, user code should use extension methods
        /// from <see cref="EligibilityExtensions"/>.
        /// </summary>
        void AddRule( IEligibilityRule<T> rule );
    }
}