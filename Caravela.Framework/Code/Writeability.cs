// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Enumerates the different abilities of a field or property to be written (set).
    /// </summary>
    [CompileTimeOnly]
    public enum Writeability
    {
        /// <summary>
        /// The field or property cannot be set (e.g. it is a read-only non-automatic property or a const field ).
        /// </summary>
        None,

        /// <summary>
        /// The field or property can only be set from the constructor (e.g. it is a <c>readonly</c> field or an automatic property with a sole <c>get</c> accessor).
        /// </summary>
        ConstructorOnly,

        /// <summary>
        /// The property can be set from constructor or from the initializer (e.g. it is a property with an <c>init</c> accessor).
        /// </summary>
        InitOnly,

        /// <summary>
        /// The field or property can be set at all times (e.g. this is a non-<c>readonly</c> field or a property with a <c>set</c> accessor).
        /// </summary>
        All
    }
}