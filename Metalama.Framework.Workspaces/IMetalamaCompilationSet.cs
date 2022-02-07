// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Introspection;
using System.Collections.Immutable;

namespace Metalama.Framework.Workspaces;

/// <summary>
/// Represents the output of the compilation of a project by Metalama. 
/// </summary>
public interface IMetalamaCompilationSet : ICompilationSet
{
    /// <summary>
    /// Gets all aspect instances in the current compilation.
    /// </summary>
    ImmutableArray<IIntrospectionAspectInstance> AspectInstances { get; }

    /// <summary>
    /// Gets the aspect classes in the current compilation.
    /// </summary>
    ImmutableArray<IIntrospectionAspectClass> AspectClasses { get; }

    /// <summary>
    /// Gets the diagnostics emitted for Metalama or by aspects in the current compilation.
    /// </summary>
    ImmutableArray<IIntrospectionDiagnostic> Diagnostics { get; }
}