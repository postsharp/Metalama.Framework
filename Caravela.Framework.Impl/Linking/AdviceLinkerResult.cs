using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal record AdviceLinkerResult(
        CSharpCompilation Compilation,
        IReadOnlyCollection<Diagnostic> Diagnostics );
}
