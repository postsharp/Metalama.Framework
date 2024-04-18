// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Validation;

/// <summary>
/// Identifies the end of the kind of <see cref="ReferenceEnd"/>.
/// </summary>
[CompileTime]
public enum ReferenceDirection : byte
{
    /// <summary>
    /// The <i>referenced</i> declaration.
    /// </summary>
    Inbound,

    /// <summary>
    /// The <i>referencing</i> declaration.
    /// </summary>
    Outbound
}