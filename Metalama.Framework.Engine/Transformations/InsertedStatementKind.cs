// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.Transformations;

public enum InsertedStatementKind
{
    /// <summary>
    /// Insert statement into the beginning of the final version of the declaration, in transformation order. 
    /// </summary>
    Initializer = -200,

    /// <summary>
    /// Insert statement into the beginning of the current version of the target declaration (source, introduction or latest override). 
    /// Statements added by one layer have their order preserved.
    /// </summary>
    InputContract = -100,

    [Obsolete]
    OutputContract = 100,

}