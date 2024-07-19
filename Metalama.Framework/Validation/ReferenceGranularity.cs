// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Validation;

/// <summary>
/// Levels of granularity on which a validation can be performed. The order of enum values are from the coarsest to the finest level.
/// The finer the granularity of a validator, the more resources it will consume.
/// </summary>
[CompileTime]
public enum ReferenceGranularity : byte
{
    /// <summary>
    /// Sets the validator granularity to the level of whole compilation.
    /// </summary>
    Compilation,

    /// <summary>
    /// Sets the validator granularity to the level of namespaces (possibly the global namespace).
    /// </summary>
    Namespace,

    /// <summary>
    /// Sets the validator granularity to the  level of top-level types, i.e. the types  directly belonging to the namespace, as opposed to a nested type.
    /// </summary>
    TopLevelType,

    /// <summary>
    /// Sets the validator granularity to the level of types, possibly nested types.
    /// </summary>
    Type,

    /// <summary>
    /// Sets the validator granularity to the level of methods, fields, events, or constructors.
    /// </summary>
    Member,

    /// <summary>
    /// Sets the validator granularity to the level of parameters, type parameters, custom attributes. 
    /// </summary>
    ParameterOrAttribute,

    /// <summary>
    /// Sets the validator granularity to the level of syntax nodes. This level exists for backward compatibility. It should not be used,
    /// as it has no benefit over the next finest level, only a performance cost.
    /// </summary> 
    [Obsolete]
    SyntaxNode
}