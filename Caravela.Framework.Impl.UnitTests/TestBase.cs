using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.UnitTests
{
    public class TestBase
    {
        public static CSharpCompilation CreateRoslynCompilation( string code, bool ignoreErrors = false )
        {
            var roslynCompilation = CSharpCompilation.Create( null! )
                .WithOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ) )
                .AddSyntaxTrees( SyntaxFactory.ParseSyntaxTree( code ) )
                .AddReferences( MetadataReference.CreateFromFile( typeof( object ).Assembly.Location ) );

            if ( !ignoreErrors )
            {
                var diagostics = roslynCompilation.GetDiagnostics();
                if ( diagostics.Any( diag => diag.Severity >= DiagnosticSeverity.Error ) )
                {
                    var lines = diagostics.Select( diag => diag.ToString() ).Prepend( "The given code produced errors:" );

                    throw new InvalidOperationException( string.Join( Environment.NewLine, lines ) );
                }
            }

            return roslynCompilation;
        }

        public static ICompilation CreateCompilation( string code )
        {
            var roslynCompilation = CreateRoslynCompilation( code );

            return CompilationFactory.CreateCompilation( roslynCompilation );
        }
    }
}
