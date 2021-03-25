// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerLinkingStepOutput
    {
        public LinkerLinkingStepOutput( CSharpCompilation finalCompilation, DiagnosticList diagnosticSink )
        {
            this.FinalCompilation = finalCompilation;
            this.DiagnosticSink = diagnosticSink;
        }

        public CSharpCompilation FinalCompilation { get; }

        public DiagnosticList DiagnosticSink { get; }
    }
}