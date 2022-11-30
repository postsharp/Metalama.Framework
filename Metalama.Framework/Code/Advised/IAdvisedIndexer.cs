// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the property being overwritten or introduced. This interface extends <see cref="IIndexer"/> but
    /// overrides the <see cref="Parameters"/> property to expose their <see cref="IExpression.Value"/> property.
    /// </summary>
    public interface IAdvisedIndexer : IIndexer, IAdvisedFieldOrPropertyOrIndexer
    {
        /// <summary>
        /// Gets the list of indexer parameters.
        /// </summary>
        new IAdvisedParameterList Parameters { get; }
    }
}