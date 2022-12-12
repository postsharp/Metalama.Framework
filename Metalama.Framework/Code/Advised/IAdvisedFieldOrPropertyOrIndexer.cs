// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the field, property or indexer being overwritten or introduced. This interface extends <see cref="IFieldOrPropertyOrIndexer"/> 
    /// but overrides the <see cref="GetMethod"/> and <see cref="SetMethod"/> members to expose their <see cref="IAdvisedMethod.Invoke"/> method.
    /// </summary>
    public interface IAdvisedFieldOrPropertyOrIndexer : IFieldOrPropertyOrIndexer
    {
        /// <summary>
        /// Gets the method implementing the <c>get</c> semantic. In case of fields, this property returns
        /// an object that does not map to source code but allows to add aspects and advice as with a normal method.
        /// </summary>
        new IAdvisedMethod? GetMethod { get; }

        /// <summary>
        /// Gets the method implementing the <c>set</c> semantic. In case of fields, this property returns
        /// an object that does not map to source code but allows to add aspects and advice as with a normal method.
        /// </summary>
        new IAdvisedMethod? SetMethod { get; }
    }
}