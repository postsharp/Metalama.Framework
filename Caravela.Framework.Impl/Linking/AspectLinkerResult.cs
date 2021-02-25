using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal record AspectLinkerResult(
        CSharpCompilation Compilation,
        IReadOnlyCollection<Diagnostic> Diagnostics );
}
