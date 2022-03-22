// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Aspects
{
    public enum InitializationReason
    {
        /// <summary>
        /// Indicates that the advice should be executed at the beginning of the instance constructor. 
        /// The advice is executed before the instance constructor code is executed.
        /// </summary>
        Constructing,

        /// <summary>
        /// Indicates that the advice should be executed at the beginning of the type constructor. 
        /// This is before the instance constructor code is executed.
        /// </summary>
        TypeConstructing,

        [Obsolete("Not implemented")]
        /// <summary>
        /// Indicates that the advice should be executed after all constructors are finished but before the initialization block.
        /// </summary>
        Constructed,

        [Obsolete( "Not implemented" )]
        /// <summary>
        /// Indicates that the advice should be executed after all constructors are finished and after the initialization block.
        /// </summary>
        Initialized,

        [Obsolete( "Not implemented" )]
        /// <summary>
        /// Indicates that the advice should be executed when the instance of a target class is deserialized.
        /// </summary>
        Deserialized,

        [Obsolete( "Not implemented" )]
        /// <summary>
        /// Indicates that the advice should be executed when the instance of a target class is cloned.
        /// </summary>
        Cloned,

        [Obsolete( "Not implemented" )]
        /// <summary>
        /// Indicated that the advice should be executed when the the target value type is mutated using the "with" expression.
        /// </summary>
        Mutated,
    }
}