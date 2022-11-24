// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the field or property or indexer being overwritten or introduced. This interface introduces
    /// the <see cref="IExpression.Value"/> property, which allows you to read or write the field or property or indexer.
    /// </summary>
    public interface IAdvisedFieldOrPropertyOrIndexer : IFieldOrPropertyOrIndexer
    {
    }
}