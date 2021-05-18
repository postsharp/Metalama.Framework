﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The main compile-time entry point of Caravela. An implementation of Caravela.Compiler's <see cref="ISourceTransformer"/>.
    /// </summary>
    internal sealed class SourceTransformer : ISourceTransformer
    {
        public Compilation Execute( TransformerContext transformerContext )
        {
            using CompileTimeAspectPipeline pipeline = new(
                new BuildOptions( transformerContext.GlobalOptions, transformerContext.Plugins ),
                new CompileTimeDomain(),
                new CompilationAssemblyLocator( transformerContext.Compilation ) );

            if ( pipeline.TryExecute(
                new DiagnosticAdder( transformerContext.ReportDiagnostic ),
                transformerContext.Compilation,
                CancellationToken.None,
                out var compilation,
                out var additionalResources ) )
            {
                transformerContext.ManifestResources.AddRange( additionalResources );

                return compilation;
            }
            else
            {
                // The pipeline failed.
                return transformerContext.Compilation;
            }
        }
    }
}