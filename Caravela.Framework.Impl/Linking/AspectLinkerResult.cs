// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
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
        /// Gets diagnostics produced when linking (templace expansion, inlining, etc.).
        /// </summary>
        public IReadOnlyCollection<Diagnostic> Diagnostics { get; }

        public AspectLinkerResult( CSharpCompilation compilation, IReadOnlyCollection<Diagnostic> diagnostics )
        {
            this.Compilation = compilation;
            this.Diagnostics = diagnostics;
        }
    }
}
