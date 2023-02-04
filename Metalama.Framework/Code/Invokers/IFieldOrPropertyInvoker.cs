// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Allows accessing the the value of fields or properties.
    /// </summary>
    [Obsolete( "Use the RunTimeInvocationExtensions extension class.", true )]
    public interface IFieldOrPropertyInvoker : IInvoker
    {
        /// <summary>
        /// Get the value for a different instance.
        /// </summary>
        dynamic GetValue( dynamic? instance );

        /// <summary>
        /// Set the value for a different instance.
        /// </summary>
        dynamic SetValue( dynamic? instance, dynamic? value );
    }
}