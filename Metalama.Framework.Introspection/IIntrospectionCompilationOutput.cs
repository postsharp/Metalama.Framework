// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System.Collections.Immutable;

namespace Metalama.Framework.Introspection;

/// <summary>
/// Represents the result of the processing of a compilation by Metalama.
/// </summary>
public interface IIntrospectionCompilationOutput
{
    /// <summary>
    /// Gets a value indicating whether the processing of the compilation by Metalama was successful.
    /// </summary>
    bool IsSuccessful { get; }

    /// <summary>
    /// Gets the list of diagnostics reported by Metalama and by aspects.
    /// </summary>
    ImmutableArray<IIntrospectionDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets the list of aspect instances in the compilation.
    /// </summary>
    ImmutableArray<IIntrospectionAspectInstance> AspectInstances { get; }

    /// <summary>
    /// Gets the list of aspect classes in the compilation.
    /// </summary>
    ImmutableArray<IIntrospectionAspectClass> AspectClasses { get; }

    /// <summary>
    /// Gets the resulting compilation.
    /// </summary>
    ICompilation Compilation { get; }
}