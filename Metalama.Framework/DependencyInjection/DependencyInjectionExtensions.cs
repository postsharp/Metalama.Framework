// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Project;

namespace Metalama.Framework.DependencyInjection;

/// <summary>
/// Extends the <see cref="IProject"/> class by exposing the options that influence the handling of <see cref="DependencyAttribute"/>.
/// </summary>
[CompileTime]
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Exposes the options that influence the handling of <see cref="DependencyAttribute"/>.
    /// </summary>
    public static DependencyInjectionOptions DependencyInjectionOptions( this IProject project ) => project.Extension<DependencyInjectionOptions>();
}