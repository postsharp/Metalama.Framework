// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal struct AspectLinkerInput
    {
        public CSharpCompilation Compilation { get; }

        public CompilationModel CompilationModel { get; }

        public IReadOnlyList<INonObservableTransformation> NonObservableTransformations { get; }

        public IReadOnlyList<OrderedAspectLayer> OrderedAspectLayers { get; }

        public AspectLinkerInput(
            CSharpCompilation compilation, 
            CompilationModel compilationModel, 
            IReadOnlyList<INonObservableTransformation> nonObservableTransformations,
            IReadOnlyList<OrderedAspectLayer> orderedAspectLayers )
        {
            this.Compilation = compilation;
            this.CompilationModel = compilationModel;
            this.NonObservableTransformations = nonObservableTransformations;
            this.OrderedAspectLayers = orderedAspectLayers;
        }
    }
}
