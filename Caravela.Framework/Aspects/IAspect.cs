// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System.Runtime.Serialization;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// The base interface for all aspects. A class should not implement
    /// this interface, but the strongly-typed variant <see cref="IAspect{T}"/>.
    /// </summary>
    [CompileTime]
    public interface IAspect
    {
        /// <summary>
        /// Configures the static characteristics of the aspect, i.e. those that do not depend on the instance state
        /// of the aspect class. Implementations are not allowed to reference non-static members.
        /// Implementations must call the implementation of the base class if it exists.
        /// </summary>
        /// <param name="builder">An object that allows the aspect to configure characteristics like
        /// description, dependencies, or layers.</param>
        /// <remarks>
        /// Do not reference instance class members in your implementation of  <see cref="BuildAspectClass"/>.
        /// Indeed, this method is called on an instance obtained using <see cref="FormatterServices.GetUninitializedObject"/>, that is,
        /// <i>without invoking the class constructor</i>.
        /// </remarks>
        void BuildAspectClass( IAspectClassBuilder builder )
#if NET5_0
        { }
#else
            ;
#endif
    }

    /// <summary>
    /// The base interface for all aspects, with the type parameter indicating to which types
    /// of declarations the aspect can be added.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAspect<in T> : IAspect, IEligible<T>
        where T : class, IDeclaration
    {
        /// <summary>
        /// Initializes the aspect. The implementation must add advices, child aspects and validators
        /// using the <paramref name="builder"/> parameter.
        /// </summary>
        /// <param name="builder">An object that allows the aspect to add advices, child aspects and validators.</param>
        void BuildAspect( IAspectBuilder<T> builder )
#if NET5_0
        { }
#else
            ;
#endif
    }
}