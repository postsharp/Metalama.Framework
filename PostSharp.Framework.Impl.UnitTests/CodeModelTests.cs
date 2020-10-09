using System;
using System.Collections.Generic;
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

        [Fact]
        public void AttributeData()
        {
            string code = @"
using System;

enum E
{
    F, G
}

[Test(42, ""foo"", null, E = E.G, Types = new[] { typeof(E), typeof(Action<,>), null })]
class TestAttribute : Attribute
{
    public TestAttribute(int i, string s, object o) {}

    public E E { get; set; }
    public Type[] Types { get; set; }
}";
            var compilation = CreateCompilation(code);

            var attribute = compilation.Types[1].Attributes.Single();
            Assert.Equal("TestAttribute", attribute.Type.FullName);
            Assert.Equal(new object?[] { 42, "foo", null }, attribute.ConstructorArguments);
            var namedArguments = attribute.NamedArguments;
            Assert.Equal(2, namedArguments.Count);
            Assert.Equal(1, namedArguments["E"]);
            var types = Assert.IsAssignableFrom<IReadOnlyList<object?>>(namedArguments["Types"]);
            Assert.Equal(3, types.Count);
            var type0 = Assert.IsAssignableFrom<INamedType>(types[0]);
            Assert.Equal("E", type0.FullName);
            var type1 = Assert.IsAssignableFrom<INamedType>(types[1]);
            Assert.Equal("System.Action<,>", type1.FullName);
            Assert.Null(types[2]);
        }

        [Fact]
        public void Parameters()
        {
            string code = @"
using System;

interface I
{
    void M1(Int32 i, in object o, out String s);
    ref readonly int M2();
}";

            var compilation = CreateCompilation(code);

            var methods = compilation.Types.Single().Methods;
            Assert.Equal(2, methods.Count);

            var m1 = methods[0];
            Assert.Equal("M1", m1.Name);

            CheckParameterData(m1.ReturnParameter, m1, "void", null, -1);
            Assert.Equal(3, m1.Parameters.Count);
            CheckParameterData(m1.Parameters[0], m1, "int", "i", 0);
            CheckParameterData(m1.Parameters[1], m1, "object", "o", 1);
            CheckParameterData(m1.Parameters[2], m1, "string", "s", 2);

            var m2 = methods[1];
            Assert.Equal("M2", m2.Name);

            CheckParameterData(m2.ReturnParameter, m2, "int", null, -1);
            Assert.Equal(0, m2.Parameters.Count);

            static void CheckParameterData(
                IParameter parameter, ICodeElement containingElement, string typeName, string? name, int index)
            {
                Assert.Same(containingElement, parameter.ContainingElement);
                Assert.Equal(typeName, ((INamedType)parameter.Type).FullName);
                Assert.Equal(name, parameter.Name);
                Assert.Equal(index, parameter.Index);
            }
        }

        [Fact]
        public void GenericArguments()
        {
            string code = @"
class C<T1, T2>
{
    static C<int, string> GetInstance() => null;
}";

            var compilation = CreateCompilation(code);

            var type = compilation.Types.Single();

            // TODO: check type.GenericArguments once ITypeParameterSymbol is supported

            var method = type.Methods[0];

            Assert.Equal("C<int, string>", method.ReturnType.ToString());
            Assert.Equal(new string[] { "int", "string" }, ((INamedType)method.ReturnType).GenericArguments.Select(t => t.ToString()));
        }
    }
}
