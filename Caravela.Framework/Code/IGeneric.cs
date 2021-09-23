// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.Collections;

namespace Caravela.Framework.Code
{
    public interface IGeneric : IMemberOrNamedType
    {
        /// <summary>
        /// Gets the generic parameters of the current method.
        /// </summary>
        IGenericParameterList GenericParameters { get; }

        /// <summary>
        /// Gets a value indicating whether this method or any of its containers does not have generic arguments set.
        /// </summary>
        bool IsOpenGeneric { get; }
    }
}