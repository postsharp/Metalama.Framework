using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PostSharp.Framework.Impl
{
    sealed class AspectCompilation
    {
        public IReadOnlyList<Diagnostic> Diagnostics { get; }
        public CSharpCompilation Compilation { get; }
        // TODO: which aspects is this supposed to contain and why?
        //public IReadOnlyList<AspectInstance> Aspects { get; }

        public AspectCompilation(IReadOnlyList<Diagnostic> diagnostics, CSharpCompilation compilation)
        {
            Diagnostics = diagnostics;
            Compilation = compilation;
        }

        public AspectCompilation Update(IReadOnlyList<Diagnostic> addedDiagnostics, CSharpCompilation newCompilation)
        {
            var newDiagnostics = Diagnostics.Concat(addedDiagnostics).ToImmutableArray();

            return new AspectCompilation(newDiagnostics, newCompilation);
        }
    }
}
