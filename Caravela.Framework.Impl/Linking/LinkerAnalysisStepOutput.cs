// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerAnalysisStepOutput
    {
        public LinkerAnalysisStepOutput( DiagnosticList diagnosticSink, CSharpCompilation intermediateCompilation, LinkerAnalysisRegistry analysisRegistry )
        {
            this.DiagnosticSink = diagnosticSink;
            this.IntermediateCompilation = intermediateCompilation;
            this.AnalysisRegistry = analysisRegistry;
        }

        public DiagnosticList DiagnosticSink { get; }

        public CSharpCompilation IntermediateCompilation { get; }

        public LinkerAnalysisRegistry AnalysisRegistry { get; }
    }
}