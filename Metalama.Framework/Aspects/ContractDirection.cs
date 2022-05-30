// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Aspects;

/// <summary>
/// Directions of the data flow to which the filter applies.
/// </summary>
[RunTimeOrCompileTime]
public enum ContractDirection
{
    /// <summary>
    /// For all parameters except <c>out</c> parameters and read-only properties or indexers, equivalent to <see cref="Input"/>. Otherwise, equivalent to <see cref="Output"/>. 
    /// </summary>
    Default,

    /// <summary>
    /// Filters the input value of the parameter (before execution of the method) or the value assigned to the field, property or indexer (before the actual assignment).
    /// </summary>
    Input,

    /// <summary>
    /// Filters the output value of an <c>out</c> or <c>ref</c> parameter or the value (after execution of the method), the value returned by the
    /// property or indexer getter, or the value assigned to the field at the moment when the field is retrieved.
    /// </summary>
    Output,

    /// <summary>
    /// Both <see cref="Input"/> and <see cref="Output"/>.
    /// </summary>
    Both
}