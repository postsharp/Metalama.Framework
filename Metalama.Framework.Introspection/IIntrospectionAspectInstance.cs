// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Collections.Immutable;

namespace Metalama.Framework.Introspection;

/// <summary>
///  Represents an instance of an aspect, as well as the results of the aspect instance.
/// </summary>
public interface IIntrospectionAspectInstance : IAspectInstance
{
    /// <summary>
    /// Gets the list of diagnostics produced by the aspect.
    /// </summary>
    ImmutableArray<IIntrospectionDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets the advice added by the aspect.
    /// </summary>
    IReadOnlyList<IIntrospectionAdvice> Advice { get; }

    /// <summary>
    /// Gets the declaration to which the aspect is applied.
    /// </summary>
    new IDeclaration TargetDeclaration { get; }
}