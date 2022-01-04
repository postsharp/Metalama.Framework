﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// The base interface for all aspects. A class should not implement
    /// this interface, but the strongly-typed variant <see cref="IAspect{T}"/>.
    /// </summary>
    [CompileTime]
    public interface IAspect : ILamaSerializable { }

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
#if NET5_0_OR_GREATER
        { }
#else
            ;
#endif
    }
}