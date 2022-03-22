// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations
{
    internal struct CodeTransformationSourceContext
    {
        public TransformationInitializationResult? InitializationResult { get; }

        public IReadOnlyDictionary<IHierarchicalTransformation, TransformationInitializationResult?> DependencyInitializationResults { get; }

        public CodeTransformationSourceContext(
            TransformationInitializationResult? initializationResult,
            IReadOnlyDictionary<IHierarchicalTransformation, TransformationInitializationResult?> dependencyInitializationResults )
        {
            this.InitializationResult = initializationResult;
            this.DependencyInitializationResults = dependencyInitializationResults;
        }
    }
}