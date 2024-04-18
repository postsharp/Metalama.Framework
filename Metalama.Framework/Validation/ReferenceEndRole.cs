// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Validation;

/// <summary>
/// Identifies the role of the <see cref="ReferenceEnd"/>.
/// </summary>
[CompileTime]
public enum ReferenceEndRole : byte
{
    /// <summary>
    /// The <i>referenced</i> declaration.
    /// </summary>
    Destination,

    /// <summary>
    /// The <i>referencing</i> declaration.
    /// </summary>
    Origin
}