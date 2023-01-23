// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the constructor being overwritten or introduced. This interface extends <see cref="IConstructor"/>.
    /// </summary>
    public interface IAdvisedConstructor : IConstructor
    {
        /// <summary>
        /// Gets the list of constructor parameters.
        /// </summary>
        new IAdvisedParameterList Parameters { get; }
    }
}