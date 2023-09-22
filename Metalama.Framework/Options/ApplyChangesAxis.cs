// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Options;

/// <summary>
/// Enumerates the axes along which two option layers can be merged by the <see cref="IIncrementalObject.ApplyChanges"/> method. 
/// </summary>
[CompileTime]
public enum ApplyChangesAxis
{
    /// <summary>
    /// Means that options directly applied to the declaration override other options also directly applied to the declaration.
    /// </summary>
    Direct,

    /// <summary>
    /// Means that options on the containing declaration (typically the declaring type, but not the namespace, which are specified by the
    /// <see cref="Namespace"/> axis) override the options defined in the base declaration.
    /// For instance, type-level options on the declaring type of an <c>override</c> method override method-level options on the <c>base</c> method.
    /// </summary>
    ContainingDeclaration,

    /// <summary>
    /// Means that options on the containing namespace override the options defined on the base declaration or on the containing declaration.
    /// </summary>
    Namespace,

    /// <summary>
    /// Means that options directly applied to the declaration override options inherited along the containment or base axis.
    /// </summary>
    Declaration,
    
    /// <summary>
    /// Means that options defined by the aspect instance itself override any other option.
    /// </summary>
    Aspect,
}