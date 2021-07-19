// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.CodeModel.Invokers
{
    /// <summary>
    /// Kinds of member access operator: <c>.</c> or <c>?.</c>.
    /// </summary>
    internal enum InvokerOperator
    {
        /// <summary>
        /// Default '.' operator.
        /// </summary>
        Default,

        /// <summary>
        /// Conditional ('?.') operator.
        /// </summary>
        Conditional
    }
}