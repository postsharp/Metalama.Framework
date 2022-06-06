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
    bool CanWeave( WeaveDependencyContext context );

    /// <summary>
    /// Weaves a given aspect dependency advice into the target code.
    /// </summary>
    /// <param name="templateMember">The field or property of the aspect that has the <see cref="DependencyAttribute"/> custom attribute.</param>
    /// <param name="templateMemberId">The a value that represents <paramref name="templateMember"/> and that must be supplied to <see cref="IAdviceFactory"/>.
    ///     It is not actually the name, but a unique identifier of <paramref name="templateMember"/>.</param>
    /// <param name="builder">The <see cref="IAspectBuilder"/> for the current aspect.</param>
    void Weave( IMemberOrNamedType templateMember, string templateMemberId, IAspectBuilder<IDeclaration> builder );
}