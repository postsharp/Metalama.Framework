// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the property being overwritten or introduced. This interface extends <see cref="IProperty"/> but introduces
    /// the <see cref="IExpression.Value"/> property, which allows you to read or write the property.
    /// </summary>
    public interface IAdvisedIndexer : IIndexer, IAdvisedFieldOrPropertyOrIndexer
    {
        /// <summary>
        /// Gets the list of indexer parameters.
        /// </summary>
        new IAdvisedParameterList Parameters { get; }
    }
}