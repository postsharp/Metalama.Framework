// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.DependencyInjection;

/// <summary>
/// Represents the context in which the an aspect dependency advice is weaved. 
/// </summary>
/// <param name="AspectFieldOrProperty">The advice field or property in the aspect type.</param>
/// <param name="AspectFieldOrPropertyId">The template name to pass to methods of <see cref="IAdviceFactory"/>.</param>
/// <param name="DependencyAttribute">The <see cref="DependencyAttribute"/> applied to <see cref="AspectFieldOrProperty"/>.</param>
/// <param name="TargetType">The type into which the dependency should be weaved.</param>
/// <param name="Diagnostics">An object that allows to report diagnostics.</param>
[CompileTime]
public sealed record WeaveDependencyContext(
    IFieldOrProperty AspectFieldOrProperty,
    string AspectFieldOrPropertyId,
    DependencyAttribute DependencyAttribute,
    INamedType TargetType,
    in ScopedDiagnosticSink Diagnostics );