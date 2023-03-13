// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Code;

/// <summary>
/// Options that determine how <see cref="IRef{T}.GetTarget"/> and related methods work.
/// </summary>
[Flags]
[CompileTime]
public enum ReferenceResolutionOptions
{
    /// <summary>
    /// An exception is thrown when the declaration does not exist in the target compilation. Redirections are followed.
    /// </summary>
    Default,

    /// <summary>
    /// Returns a representation of the declaration even if the declaration does not exist in that compilation.
    /// </summary>
    CanBeMissing = 1,

    /// <summary>
    /// Do not follow redirections.
    /// </summary>
    DoNotFollowRedirections = 2
}