// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.Transformations;

public enum InsertedStatementKind
{
    /// <summary>
    /// Insert statement into the beginning of the final version of the declaration. 
    /// If both FinalStart and CurrentStart statements are inserted to one body, FinalStart precede CurrentStart.
    /// </summary>
    FinalEntry = -100, // This is used by initializers.

    /// <summary>
    /// Insert statement into the beginning of the current version of the declaration (i.e. source, empty declaration or latest overload).
    /// </summary>
    [Obsolete( "Not implemented" )] // This is intended for parameter contracts.
    CurrentEntry = 0,
}