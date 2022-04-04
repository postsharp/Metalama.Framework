// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Kinds of parameters, such as <c>ref</c>, <c>in</c>, <c>out</c>...
    /// </summary>
    [CompileTime]
    public enum RefKind
    {
        /// <summary>
        /// A normal parameter passed by value.
        /// </summary>
        None,

        /// <summary>
        /// <c>ref</c>.
        /// </summary>
        Ref,

        /// <summary>
        /// <c>in</c> input parameter. Synonym of <see cref="RefReadOnly"/>.
        /// </summary>
        In,

        /// <summary>
        /// <c>ref readonly</c> property or return parameter.
        /// </summary>
        RefReadOnly = In,

        /// <summary>
        /// <c>out</c>.
        /// </summary>
        Out
    }
}