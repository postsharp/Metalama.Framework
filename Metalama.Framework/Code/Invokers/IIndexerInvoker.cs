// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Allows accessing the value of indexers.
    /// </summary>
    [CompileTime]
#pragma warning disable CS0612
    public interface IIndexerInvoker : IInvoker
#pragma warning restore CS0612
    {
        /// <summary>
        /// Get the value for an indexer.
        /// </summary>
        dynamic? GetValue( params dynamic?[] args );

        /// <summary>
        /// Set the value for an indexer.
        /// </summary>
        /// <remarks>
        /// Note: the order of parameters is different than in C# code:
        /// e.g. <c>instance[args] = value</c> is <c>indexer.SetIndexerValue(instance, value, args)</c>.
        /// </remarks>
        dynamic? SetValue( dynamic? value, params dynamic?[] args );

        IIndexerInvoker With( InvokerOptions options );

        IIndexerInvoker With( dynamic? target, InvokerOptions options = default );
    }
}