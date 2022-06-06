// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.DependencyInjection;

/// <summary>
/// Interface that dependency injection framework adapters should implement to pull a specific field or property from the constructor.
/// </summary>
[CompileTime]
public interface IPullStrategy
{
    /// <summary>
    /// Gets the action required to pull a specified field or property from a specified constructor.
    /// </summary>
    /// <param name="fieldOrProperty">The generated field or property that needs to be initialized.</param>
    /// <param name="constructor">The constructor from which <paramref name="fieldOrProperty"/> should be initialized.</param>
    /// <param name="diagnostics">Allows to report diagnostics.</param>
    PullAction PullFieldOrProperty( IFieldOrProperty fieldOrProperty, IConstructor constructor, in ScopedDiagnosticSink diagnostics );

    /// <summary>
    /// Gets the action required to pull a specified constructor parameter from a different constructor, called with either the <c>base</c> or <c>this</c>
    /// keyword.
    /// </summary>
    /// <param name="parameter">The parameter of the base constructor that needs to be pulled.</param>
    /// <param name="constructor">The constructor that calls the base constructor that contains <paramref name="parameter"/>.</param>
    PullAction PullParameter( IParameter parameter, IConstructor constructor, in ScopedDiagnosticSink diagnostics );
}