// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code.Invokers
{
    /// <summary>
    /// Allows accessing the the value of fields or properties.
    /// </summary>
    [CompileTimeOnly]
    public interface IFieldOrPropertyInvoker : IInvoker
    {
        /// <summary>
        /// Get the value for a different instance.
        /// </summary>
        [return: RunTimeOnly]
        dynamic GetValue( dynamic? instance );

        /// <summary>
        /// Set the value for a different instance.
        /// </summary>
        [return: RunTimeOnly]
        dynamic SetValue( dynamic? instance, dynamic value );
    }
}