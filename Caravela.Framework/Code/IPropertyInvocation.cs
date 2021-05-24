// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows accessing the value of properties and indexers.
    /// </summary>
    public interface IPropertyInvocation : IFieldOrPropertyInvocation
    {
        /// <summary>
        /// Get the value for an indexer.
        /// </summary>
        [return: RunTimeOnly]
        dynamic GetIndexerValue( dynamic? instance, params dynamic[] args );

        /// <summary>
        /// Set the value for an indexer.
        /// </summary>
        /// <remarks>
        /// Note: the order of parameters is different than in C# code:
        /// e.g. <c>instance[args] = value</c> is <c>indexer.SetIndexerValue(instance, value, args)</c>.
        /// </remarks>
        [return: RunTimeOnly]
        dynamic SetIndexerValue( dynamic? instance, dynamic value, params dynamic[] args );
    }
}