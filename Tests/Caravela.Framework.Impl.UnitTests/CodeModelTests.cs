using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Xunit;
using static Caravela.Framework.Code.MethodKind;
using static Caravela.Framework.Code.TypeKind;
using static Caravela.Framework.Code.RefKind;

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

interface I<T>
{
    void M1(Int32 i, T t, dynamic d, in object o, out String s);
    ref readonly int M2();
}";

            var compilation = CreateCompilation(code);

            var methods = compilation.DeclaredTypes.GetValue().Single().Methods.GetValue().ToList();
            Assert.Equal(2, methods.Count);

            var m1 = methods[0];
            Assert.Equal("M1", m1.Name);

            CheckParameterData(m1.ReturnParameter!, m1, "void", null, -1);
            Assert.Equal(5, m1.Parameters.Count);
            CheckParameterData(m1.Parameters[0], m1, "int", "i", 0);
            CheckParameterData(m1.Parameters[1], m1, "T", "t", 1);
            CheckParameterData(m1.Parameters[2], m1, "dynamic", "d", 2);
            CheckParameterData(m1.Parameters[3], m1, "object", "o", 3);
            CheckParameterData(m1.Parameters[4], m1, "string", "s", 4);

            var m2 = methods[1];
            Assert.Equal("M2", m2.Name);

            CheckParameterData(m2.ReturnParameter!, m2, "int", null, -1);
            Assert.Equal(0, m2.Parameters.Count);

            static void CheckParameterData(
                IParameter parameter, ICodeElement containingElement, string typeName, string? name, int index)
            {
                Assert.Same(containingElement, parameter.ContainingElement);
                Assert.Equal(typeName, parameter.Type.ToString());
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

            Assert.Equal( new[] { "T1", "T2" }, type.GenericArguments.Select( t => t.ToString() ) );

            var method = type.Methods.GetValue().First();

            Assert.Equal("C<int, string>", method.ReturnType.ToString());
            Assert.Equal(new[] { "int", "string" }, ((INamedType)method.ReturnType).GenericArguments.Select(t => t.ToString()));
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

            var attributes = compilation.Attributes.GetValue().ToArray();

            Assert.Equal( 2, attributes.Length );

            Assert.Equal( "MyAttribute", attributes[0].Type.FullName );
            Assert.Equal( "a", Assert.Single( attributes[0].ConstructorArguments ) );

            Assert.Equal( "MyAttribute", attributes[1].Type.FullName );
            Assert.Equal( "m", Assert.Single( attributes[1].ConstructorArguments ) );
        }

        [Fact]
        public void Arrays()
        {
            string code = @"
class C
{
    void M(int[] i) {}
}
";

            var compilation = CreateCompilation( code );

            var parameterTypes = from type in compilation.DeclaredTypes
                                 from method in type.Methods
                                 from parameter in method.Parameters
                                 select parameter.Type;
            var parameterType = Assert.Single( parameterTypes.GetValue() );

            Assert.Equal( "int[]", parameterType.ToString() );
            Assert.True( parameterType.Is( typeof( int[] ) ) );
            Assert.False( parameterType.Is( typeof( int[,] ) ) );

            var arrayType = Assert.IsAssignableFrom<IArrayType>( parameterType );

            Assert.Equal( "int", arrayType.ElementType.ToString() );
            Assert.True( arrayType.ElementType.Is( typeof( int ) ) );
            Assert.Equal( 1, arrayType.Rank );
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
    int field;
}";

            var compilation = CreateCompilation(code);

            var type = Assert.Single( compilation.DeclaredTypes.GetValue() );

            var propertyNames = type.Properties.Select( p => p.Name ).GetValue();

            Assert.Equal( new[] { "Auto", "GetOnly", "ReadWrite", "ReadOnly", "WriteOnly", "field" }, propertyNames );
        }

        [Fact]
        public void RefProperties()
        {
            string code = @"
class C
{
    int field;

    int None { get; set; }
    ref int Ref => ref field;
    ref readonly int RefReadonly => ref field;
}";

            var compilation = CreateCompilation( code );

            var type = Assert.Single( compilation.DeclaredTypes.GetValue() );

            var refKinds = type.Properties.Select( p => p.RefKind ).GetValue();

            Assert.Equal( new[] { None, None, Ref, RefReadonly }, refKinds );
        }

        [Fact]
        public void MethodKinds()
        {
            string code = @"
using System;
class C : IDisposable
{
	void M()
	{
		void L() { }
	}
	C() { }
	static C() { }
	~C() { }
	int P { get; set; }
	event EventHandler E { add {} remove {} }
	void IDisposable.Dispose() { }
	public static explicit operator int(C c) => 42;
	public static C operator -(C c) => c;
}";

            var compilation = CreateCompilation( code );

            var type = Assert.Single( compilation.DeclaredTypes.GetValue() );

            var methodKinds = new[] {
                Default,
                Constructor, StaticConstructor, Finalizer,
                PropertyGet, PropertySet,
                EventAdd, EventRemove,
                ExplicitInterfaceImplementation,
                ConversionOperator, UserDefinedOperator
            };

            Assert.Equal( methodKinds, type.Methods.Select( m => m.Kind ).GetValue() );

            Assert.Equal( LocalFunction, type.Methods.GetValue().First().LocalFunctions.Single().Kind );
        }

        [Fact]
        public void TypeKinds()
        {
            string code = @"
using System;
class C<T>
{
    int[] arr;
    C<T> c;
    Action a;
    dynamic d;
    DayOfWeek e;
    T t;
    IDisposable i;
    unsafe void* p;
    int s;
}";

            var compilation = CreateCompilation( code );

            var type = Assert.Single( compilation.DeclaredTypes.GetValue() );

            var typeKinds = new[] { Array, Class, Delegate, Dynamic, Enum, GenericParameter, Interface, Pointer, Struct };

            Assert.Equal( typeKinds, type.Properties.Select( p => p.Type.Kind ).GetValue() );
        }

        [Fact]
        public void ParameterKinds()
        {
            string code = @"
class C
{
    int i;

    void M1(int i, in int j, ref int k, out int m) => m = 0;
    ref int M2() => ref i;
    ref readonly int M3 => ref i;
}";

            var compilation = CreateCompilation( code );

            var type = Assert.Single( compilation.DeclaredTypes.GetValue() );

            Assert.Equal( new[] { None, In, Ref, Out }, type.Methods.GetValue().First().Parameters.Select( p => p.RefKind ) );
            Assert.Equal( new RefKind?[] { None, Ref, RefReadonly, null }, type.Methods.GetValue().Select(m => m.ReturnParameter?.RefKind) );
        }

        [Fact]
        public void ParameterDefaultValue()
        {
            string code = @"
using System;

class C
{
    void M(int i, int j = 42, string s = ""forty two"", decimal d = 3.14m, DateTime dt = default, DateTime? dt2 = null, object o = null) {}
}";

            var compilation = CreateCompilation( code );

            var type = Assert.Single( compilation.DeclaredTypes.GetValue() );

            var method = type.Methods.GetValue().First();

            var parametersWithoutDefaults = new[] { method.ReturnParameter!, method.Parameters[0] };

            foreach ( var parameter in parametersWithoutDefaults )
            {
                Assert.False( parameter.HasDefaultValue );
                Assert.Throws<System.InvalidOperationException>( () => parameter.DefaultValue );
            }

            var parametersWithDefaults = method.Parameters.Skip( 1 );

            foreach ( var parameter in parametersWithDefaults )
            {
                Assert.True( parameter.HasDefaultValue );
            }

            Assert.Equal( new object?[] { 42, "forty two", 3.14m, null, null, null }, parametersWithDefaults.Select( p => p.DefaultValue ) );
        }

        [Fact]
        public void GetTypeByReflectionType()
        {
            var compilation = CreateCompilation( null );

            Assert.Equal( "System.Collections.Generic.List<T>.Enumerator", compilation.GetTypeByReflectionType( typeof( List<>.Enumerator ) )!.ToString() );
            Assert.Equal( "System.Collections.Generic.Dictionary<int, string>", compilation.GetTypeByReflectionType( typeof( Dictionary<int, string> ) )!.ToString() );
            Assert.Equal( "int[][*,*]", compilation.GetTypeByReflectionType( typeof( int[][,] ) )!.ToString() );
            Assert.Equal( "void*", compilation.GetTypeByReflectionType( typeof( void* ) )!.ToString() );

            Assert.Throws<System.ArgumentException>( () => compilation.GetTypeByReflectionType( typeof( int ).MakeByRefType() ) );
        }

        [Fact]
        public void TypeName()
        {
            string code = @"
using System.Collections.Generic;

class C<T>
{
    int i;
    List<T>.Enumerator e;
    Dictionary<int, string> d;
    (int i, int j) t;
}";

            var compilation = CreateCompilation( code );

            var type = Assert.Single( compilation.DeclaredTypes.GetValue() );

            var fieldTypes = type.Properties.Select( p => (INamedType) p.Type ).GetValue();

            Assert.Equal( new[] { "Int32", "Enumerator", "Dictionary", "ValueTuple" }, fieldTypes.Select( t => t.Name ) );
            Assert.Equal( new[] { "int", "System.Collections.Generic.List<T>.Enumerator", "System.Collections.Generic.Dictionary<int, string>", "(int i, int j)" }, fieldTypes.Select( t => t.FullName ) );
        }
    }
}
