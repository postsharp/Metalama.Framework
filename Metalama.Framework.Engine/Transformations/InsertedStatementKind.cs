// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Transformations;

public enum InsertedStatementKind
{
    /// <summary>
    /// Insert statement into the beginning of the final version of the declaration, in transformation order. 
    /// </summary>
    Initializer = -200, // This is used by initializers.

    /// <summary>
    /// Insert statement into the beginning of the current version of the declaration (source, introduction or latest override). Statements added by one layer have their order preserved.
    /// </summary>
    CurrentEntry = 0, // Used by constructor parameter contracts.
}