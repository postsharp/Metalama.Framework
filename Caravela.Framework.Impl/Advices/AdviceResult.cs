// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Advices
{
    internal record AdviceResult(
        ImmutableArray<Diagnostic> Diagnostics,
        ImmutableArray<IObservableTransformation> ObservableTransformations,
        ImmutableArray<INonObservableTransformation> NonObservableTransformations );
}