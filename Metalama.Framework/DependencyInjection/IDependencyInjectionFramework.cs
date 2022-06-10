// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.DependencyInjection;

/// <summary>
/// Interface that dependency injection framework adapters must implement to handle the <see cref="DependencyAttribute"/> advice.
/// An implementation typically also implements <see cref="IPullStrategy"/>.
/// </summary>
[CompileTime]
public interface IDependencyInjectionFramework
{
    /// <summary>
    /// Determines whether the current instance can weave a given aspect dependency advice into a given type. The implementation can
    /// report diagnostics to <see cref="WeaveDependencyContext.Diagnostics"/>.
    /// </summary>
    bool CanInjectDependency( WeaveDependencyContext context );

    /// <summary>
    /// Weaves a given aspect dependency advice into the target code.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="builder">The <see cref="IAspectBuilder"/> for the current aspect.</param>
    void InjectDependency( WeaveDependencyContext context, IAspectBuilder<INamedType> builder );
}