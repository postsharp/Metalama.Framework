// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Code
{
  
    /// <summary>
    /// A list of named arguments exposed by <see cref="IAttribute.NamedArguments"/>.
    /// </summary>
    /// <remarks>
    /// This interface does not have dictionary semantics because the order of setting members must be preserved.
    /// </remarks>
    public interface INamedArgumentList : IReadOnlyList<KeyValuePair<string, TypedConstant>>
    {
        /// <summary>
        /// Tries to get a named argument and returns <c>true</c> if such argument was defined, even if its value was set to <c>null</c>.
        /// </summary>
        /// <param name="name">Member name.</param>
        /// <param name="value">Member alue.</param>
        /// <returns></returns>
        bool TryGetByName( string name, out TypedConstant value );

        /// <summary>
        /// Gets a named argument. The caller of this method is not able to differentiate between a missing value and a value set to <c>null</c>.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object? GetValue( string name );
    }
}