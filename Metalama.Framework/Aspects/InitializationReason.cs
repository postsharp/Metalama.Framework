// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Aspects
{
    [Flags]
    public enum InitializationReason
    {
        /// <summary>
        /// Indicates that the advice should be executed at the beginning of the instance constructor. 
        /// The advice is executed before the instance constructor code is executed.
        /// </summary>
        Constructing = 0x01,

        /// <summary>
        /// Indicates that the advice should be executed at the beginning of the type constructor. 
        /// This is before the instance constructor code is executed.
        /// </summary>
        TypeConstructing = 0x02,

        [Obsolete("Not implemented")]
        /// <summary>
        /// Indicates that the advice should be executed after all constructors are finished but before the initialization block.
        /// </summary>
        Constructed = 0x04,

        [Obsolete( "Not implemented" )]
        /// <summary>
        /// Indicates that the advice should be executed after all constructors are finished and after the initialization block.
        /// </summary>
        Initialized = 0x08,

        [Obsolete( "Not implemented" )]
        /// <summary>
        /// Indicates that the advice should be executed when the instance of a target class is deserialized.
        /// </summary>
        Deserialized = 0x10,

        [Obsolete( "Not implemented" )]
        /// <summary>
        /// Indicates that the advice should be executed when the instance of a target class is cloned.
        /// </summary>
        Cloned = 0x20,

        [Obsolete( "Not implemented" )]
        /// <summary>
        /// Indicated that the advice should be executed when the the target value type is mutated using the "with" expression.
        /// </summary>
        Mutated = 0x40,
    }
}