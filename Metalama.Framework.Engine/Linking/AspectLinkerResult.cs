// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Result of the aspect linker.
    /// </summary>
    internal record AspectLinkerResult
    {
        /// <summary>
        /// Gets the final compilation.
        /// </summary>
        public PartialCompilation Compilation { get; }

        /// <summary>
        /// Gets diagnostics produced when linking (template expansion, inlining, etc.).
        /// </summary>
        public ImmutableUserDiagnosticList Diagnostics { get; }

        public AspectLinkerResult( PartialCompilation compilation, ImmutableUserDiagnosticList diagnostics )
        {
            this.Compilation = compilation;
            this.Diagnostics = diagnostics;
        }
    }
}