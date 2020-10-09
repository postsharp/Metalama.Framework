using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace PostSharp.Framework.Impl.UnitTests
{
    public class CodeModelTests
    {
        public static ICompilation CreateCompilation(string code)
        {
            var roslynCompilation = CSharpCompilation.Create(null!)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(code))
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var diagostics = roslynCompilation.GetDiagnostics();
            if (diagostics.Any(diag => diag.Severity >= DiagnosticSeverity.Error))
            {
                var lines = diagostics.Select(diag => diag.ToString()).Prepend("The given code produced errors:");

                throw new InvalidOperationException(string.Join(Environment.NewLine, lines));
            }

            return CodeModel.CreateCompilation(roslynCompilation);
        }

        [Fact]
        public void ObjectIdentity()
        {
            string code = "";
            var compilation = CreateCompilation(code);

            Assert.Same(compilation.Types, compilation.Types);
        }

        [Fact]
        public void LocalFunctions()
        {
            string code = @"
class C
{
    void M()
    {
        void Outer()
        {
            void Inner() {}
        }
    }
}";

            var compilation = CreateCompilation(code);

            var type = compilation.Types.Single();
            Assert.Equal("C", type.Name);

            var methods = type.Methods;
            Assert.Equal(2, methods.Count);

            var ctor = methods[1];
            Assert.Equal(".ctor", ctor.Name);

            var method = methods[0];
            Assert.Equal("M", method.Name);

            var outerLocalFunction = method.LocalFunctions.Single();
            Assert.Equal("Outer", outerLocalFunction.Name);

            var innerLocalFunction = outerLocalFunction.LocalFunctions.Single();
            Assert.Equal("Inner", innerLocalFunction.Name);
        }
    }
}
