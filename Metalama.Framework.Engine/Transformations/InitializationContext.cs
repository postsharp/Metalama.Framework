// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Allows the transformation to initialize itself and pass data to further parts of the system.
    /// </summary>
    internal readonly struct InitializationContext
    {
        public UserDiagnosticSink DiagnosticSink { get; }

        public IntroductionNameProvider IntroductionNameProvider { get; }

        public IReadOnlyDictionary<IHierarchicalTransformation, TransformationInitializationResult?> DependencyInitializationResults { get; }

        public InitializationContext(
            UserDiagnosticSink diagnosticSink,
            IntroductionNameProvider introductionNameProvider,
            IReadOnlyDictionary<IHierarchicalTransformation, TransformationInitializationResult?> dependencyInitializationResults )
        {
            this.DiagnosticSink = diagnosticSink;
            this.IntroductionNameProvider = introductionNameProvider;
            this.DependencyInitializationResults = dependencyInitializationResults;
        }
    }
}