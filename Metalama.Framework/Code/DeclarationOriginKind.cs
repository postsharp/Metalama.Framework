// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code;

/// <summary>
/// Kinds of <see cref="IDeclarationOrigin"/>.
/// </summary>
[CompileTime]
public enum DeclarationOriginKind
{
    /// <summary>
    /// Indicates that the declaration belongs to the current project and stems from source code and not from a source generator.
    /// </summary>
    Source,

    /// <summary>
    /// Indicates that the declaration stems from a different project or assembly.
    /// </summary>
    External,

    /// <summary>
    /// Indicates that the declaration was created by an aspect of the current project or an inherited aspect applied to the current project.
    /// </summary>
    Aspect,

    /// <summary>
    /// Indicates that the declaration belongs to the current project and has been generated by a source generator. Note that such code is
    /// not visible at design time.
    /// </summary>
    Generator
}