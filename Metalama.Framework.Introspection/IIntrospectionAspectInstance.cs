// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
    /// Gets the declarations introduced by the aspect.
    /// </summary>
    ImmutableArray<IDeclaration> IntroducedMembers { get; }

    /// <summary>
    /// Gets the declaration to which the aspect is applied.
    /// </summary>
    new IDeclaration TargetDeclaration { get; }
}