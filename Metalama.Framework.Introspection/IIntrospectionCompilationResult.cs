// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Immutable;

namespace Metalama.Framework.Introspection;

/// <summary>
/// Represents the result of the processing of a compilation by Metalama.
/// </summary>
public interface IIntrospectionCompilationResult : IIntrospectionCompilationDetails
{
    string Name { get; }
    
    /// <summary>
    /// Gets a value indicating whether the processing of the compilation by Metalama was successful.
    /// </summary>
    bool IsSuccessful { get; }

    /// <summary>
    /// Gets the resulting compilation.
    /// </summary>
    ICompilation TransformedCode { get; }
}

public interface IIntrospectionCompilationDetails
{
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

    ImmutableArray<IIntrospectionAdvice> Advice { get; }

    /// <summary>
    /// Gets the list of transformations applied to source code.
    /// </summary>
    ImmutableArray<IIntrospectionTransformation> Transformations { get; }
}