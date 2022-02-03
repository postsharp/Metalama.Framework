// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Introspection;
using System.Collections.Immutable;

namespace Metalama.Framework.Workspaces;

public interface IMetalamaCompilationSet : ICompilationSet
{
    ImmutableArray<IIntrospectionAspectInstance> AspectInstances { get; }

    ImmutableArray<IIntrospectionAspectClass> AspectClasses { get; }

    ImmutableArray<IIntrospectionDiagnostic> Diagnostics { get; }
}