// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal record AspectLinkerResult 
    {
        public CSharpCompilation Compilation { get; }

        public IReadOnlyCollection<Diagnostic> Diagnostics { get; }

        public AspectLinkerResult(CSharpCompilation compilation, IReadOnlyCollection<Diagnostic> diagnostics)
        {
            this.Compilation = compilation;
            this.Diagnostics = diagnostics;
        }
    }
}
