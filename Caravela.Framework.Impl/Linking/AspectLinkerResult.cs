// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Result of the aspect linker.
    /// </summary>
    internal record AspectLinkerResult
    {
        /// <summary>
        /// Gets the final compilation.
        /// </summary>
        public CSharpCompilation Compilation { get; }

        /// <summary>
        /// Gets diagnostics produced when linking (template expansion, inlining, etc.).
        /// </summary>
        public ImmutableDiagnosticList Diagnostics { get; }

        public AspectLinkerResult( CSharpCompilation compilation, ImmutableDiagnosticList diagnostics )
        {
            this.Compilation = compilation;
            this.Diagnostics = diagnostics;
        }
    }
}