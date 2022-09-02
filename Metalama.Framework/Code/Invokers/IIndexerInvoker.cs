// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Allows accessing the value of indexers.
    /// </summary>
    public interface IIndexerInvoker : IInvoker
    {
        /// <summary>
        /// Get the value for an indexer.
        /// </summary>
        dynamic GetValue( dynamic? instance, params dynamic?[] args );

        /// <summary>
        /// Set the value for an indexer.
        /// </summary>
        /// <remarks>
        /// Note: the order of parameters is different than in C# code:
        /// e.g. <c>instance[args] = value</c> is <c>indexer.SetIndexerValue(instance, value, args)</c>.
        /// </remarks>
        dynamic SetValue( dynamic? instance, dynamic value, params dynamic?[] args );
    }
}