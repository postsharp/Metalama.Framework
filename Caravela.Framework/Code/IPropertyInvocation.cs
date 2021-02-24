// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows accessing the value of properties and indexers.
    /// </summary>
    public interface IPropertyInvocation
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

        /// <summary>
        /// Get the value for an indexer.
        /// </summary>
        dynamic GetIndexerValue( dynamic? instance, params dynamic[] args );

        /// <summary>
        /// Set the value for an indexer.
        /// </summary>
        /// <remarks>
        /// Note: the order of parameters is different than in C# code:
        /// e.g. <c>instance[args] = value</c> is <c>indexer.SetIndexerValue(instance, value, args)</c>.
        /// </remarks>
        dynamic SetIndexerValue( dynamic? instance, dynamic value, params dynamic[] args );
    }
}