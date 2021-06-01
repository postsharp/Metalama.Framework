// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Eligibility
{
    public interface IEligible<in T>
        where T : class, IDeclaration
    {
        /// <summary>
        /// Configures the eligibility of the aspect or attribute.
        /// Implementations are not allowed to reference non-static members.
        /// Implementations must call the implementation of the base class if it exists.
        /// </summary>
        /// <param name="builder">An object that allows the aspect to configure characteristics like
        /// description, dependencies, or layers.</param>
        void BuildEligibility( IEligibilityBuilder<T> builder )
#if NETCOREAPP3_1
        { }
#else
            ;
#endif
    }
}