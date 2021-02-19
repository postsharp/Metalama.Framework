using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    internal record AspectLinkerResult(
        CSharpCompilation Compilation,
        IReadOnlyCollection<Diagnostic> Diagnostics );
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
}
