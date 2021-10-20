// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Eligibility
{
    /// <summary>
    /// An interface that allows aspect to specify to which declarations they are allowed to be applied.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [CompileTimeOnly]
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
#if NET5_0
        { }
#else
            ;
#endif
    }
}