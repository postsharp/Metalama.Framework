using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{

    // for testing
    internal static class CompilationFactory
    {
        public static ICompilation CreateCompilation( CSharpCompilation roslynCompilation ) => new SourceCompilation( roslynCompilation );
    }
}
