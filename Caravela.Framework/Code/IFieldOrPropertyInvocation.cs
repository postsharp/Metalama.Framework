// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows accessing the the value of fields or properties.
    /// </summary>
    public interface IFieldOrPropertyInvocation
    {
        dynamic Value { get; set; }

        /// <summary>
        /// Get the value for a different instance.
        /// </summary>
        dynamic GetValue( dynamic? instance );

        /// <summary>
        /// Set the value for a different instance.
        /// </summary>
        dynamic SetValue( dynamic? instance, dynamic value );
    }
}