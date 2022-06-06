// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Aspects
{
    [CompileTime]
    public enum InitializerKind
    {
        /// <summary>
        /// Indicates that the advice should be executed before any user code in all instance constructors except those that are chained to a constructor of the current class (using the <c>this</c> chaining keyword). The initialization logic executes
        /// after the call to the base constructor.
        /// </summary>
        BeforeInstanceConstructor,

        /// <summary>
        /// Indicates that the advice should be executed before the type constructor (aka static constructor) of the target type. If there is no type constructor, this advice adds one.
        /// </summary>
        BeforeTypeConstructor,

        /// <summary>
        /// Indicates that the advice should be executed after all constructors are finished but before the initialization block.
        /// </summary>
        [Obsolete( "Not implemented", true )]
        AfterLastInstanceConstructor,

        /// <summary>
        /// Indicates that the advice should be executed after all constructors are finished and after the initialization block.
        /// </summary>
        [Obsolete( "Not implemented", true )]
        AfterObjectInitialization,

        /// <summary>
        /// Indicates that the advice should be executed when the instance of a target class is deserialized.
        /// </summary>
        [Obsolete( "Not implemented", true )]
        AfterDeserialize,

        /// <summary>
        /// Indicates that the advice should be executed when the instance of a target class is cloned.
        /// </summary>
        [Obsolete( "Not implemented", true )]
        AfterMemberwiseClone,

        /// <summary>
        /// Indicated that the advice should be executed when the the target value type is mutated using the "with" expression.
        /// </summary>
        [Obsolete( "Not implemented", true )]
        AfterWith
    }
}