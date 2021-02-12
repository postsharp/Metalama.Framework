
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    internal record AdviceLinkerResult(
        CSharpCompilation Compilation,
        IReadOnlyCollection<Diagnostic> Diagnostics );
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
}
