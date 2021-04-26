// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Caravela.TestFramework
{
    public partial class AspectTestRunner
    {
        private class AspectTestPipelineContext : IAspectPipelineContext, IDiagnosticAdder
        {
            private readonly TestResult _testResult;

            public AspectTestPipelineContext( TestResult testResult )
            {
                if ( testResult.InitialCompilation == null )
                {
                    throw new ArgumentOutOfRangeException( nameof(testResult), $"{nameof(TestResult.InitialCompilation)} should not be null." );
                }

                this.Compilation = (CSharpCompilation) testResult.InitialCompilation!;
                this._testResult = testResult;
                this.ManifestResources = new List<ResourceDescription>();
            }

            public CSharpCompilation Compilation { get; }

            ImmutableArray<object> IAspectPipelineContext.Plugins => ImmutableArray<object>.Empty;

            public IList<ResourceDescription> ManifestResources { get; }

            CancellationToken IAspectPipelineContext.CancellationToken => CancellationToken.None;

            IBuildOptions IAspectPipelineContext.BuildOptions { get; } = new TestBuildOptions();

            void IDiagnosticAdder.ReportDiagnostic( Diagnostic diagnostic ) => this._testResult.ReportDiagnostic( diagnostic );

            public bool HandleExceptions => false;
        }
    }
}