using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{

    // for testing
    internal static class CompilationFactory
    {
        public static ICompilation CreateCompilation( CSharpCompilation roslynCompilation ) => new SourceCompilationModel( roslynCompilation );
    }
}
