using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{

    public class CodeModelTests : TestBase
    {
        [Fact]
        public void ObjectIdentity()
        {
            string code = "";
            var compilation = CreateCompilation(code);

            Assert.Same(compilation.DeclaredTypes, compilation.DeclaredTypes);
        }

        [Fact]
        public void TypeInfos()
        {
            string code = @"
class C
{
    class D { }
}

namespace NS
{
    class C {}
}";

            var compilation = CreateCompilation(code);

            var types = compilation.DeclaredTypes.GetValue(default).ToList();
            Assert.Equal(2, types.Count);

            var c1 = types[0];
            Assert.Equal("C", c1.Name);
            Assert.Equal("C", c1.FullName);
            Assert.Null(c1.ContainingElement);

            var d = c1.NestedTypes.GetValue().Single();
            Assert.Equal("D", d.Name);
            Assert.Equal("C.D", d.FullName);
            Assert.Same(c1, d.ContainingElement);

            var c2 = types[1];
            Assert.Equal("C", c2.Name);
            Assert.Equal("NS.C", c2.FullName);
            Assert.Null(c2.ContainingElement);
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

            var type = compilation.DeclaredTypes.GetValue(default).Single();
            Assert.Equal("C", type.Name);

            var methods = type.Methods.GetValue().ToList();
            Assert.Single( methods );

            var method = methods[0];
            Assert.Equal("M", method.Name);
            Assert.Same(type, method.ContainingElement);

            var outerLocalFunction = method.LocalFunctions.Single();
            Assert.Equal("Outer", outerLocalFunction.Name);
            Assert.Same(method, outerLocalFunction.ContainingElement);

            var innerLocalFunction = outerLocalFunction.LocalFunctions.Single();
            Assert.Equal("Inner", innerLocalFunction.Name);
            Assert.Same(outerLocalFunction, innerLocalFunction.ContainingElement);
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

            var attribute = compilation.DeclaredTypes.GetValue(default).ElementAt(1).Attributes.GetValue(default).Single();
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

            var methods = compilation.DeclaredTypes.GetValue().Single().Methods.GetValue().ToList();
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

            var type = compilation.DeclaredTypes.GetValue().Single();

            // TODO: check type.GenericArguments once ITypeParameterSymbol is supported

            var method = type.Methods.GetValue().Single();

            Assert.Equal("C<int, string>", method.ReturnType.ToString());
            Assert.Equal(new string[] { "int", "string" }, ((INamedType)method.ReturnType).GenericArguments.Select(t => t.ToString()));
        }

        [Fact]
        public void GlobalAttributes()
        {
            string code = @"
using System;

[module: MyAttribute(""m"")]
[assembly: MyAttribute(""a"")]

class MyAttribute : Attribute
{
    public MyAttribute(string target) {}
}
";

            var compilation = CreateCompilation( code );

            var attributes = compilation.GlobalAttributes.GetValue().ToArray();

            Assert.Equal( 2, attributes.Length );

            Assert.Equal( "MyAttribute", attributes[0].Type.FullName );
            Assert.Equal( "a", Assert.Single( attributes[0].ConstructorArguments ) );

            Assert.Equal( "MyAttribute", attributes[1].Type.FullName );
            Assert.Equal( "m", Assert.Single( attributes[1].ConstructorArguments ) );
        }

        [Fact]
        public void Properties()
        {
            string code = @"
class C
{
    int Auto { get; set; }
    int GetOnly { get; }
    int ReadWrite { get => 0; set {} }
    int ReadOnly { get => 0; }
    int WriteOnly { set {} }
}";

            var compilation = CreateCompilation(code);

            // TODO
        }

        [Fact]
        public void MethodKinds()
        {
            string code = @"";

            var compilation = CreateCompilation(code);

            // TODO
        }
    }
}
