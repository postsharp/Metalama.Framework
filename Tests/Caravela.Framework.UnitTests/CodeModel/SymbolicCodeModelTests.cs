using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using System;
using Xunit;
using static Caravela.Framework.Code.MethodKind;
using static Caravela.Framework.Code.RefKind;
using static Caravela.Framework.Code.TypeKind;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Caravela.Framework.UnitTests
{
    public class SymbolicCodeModelTests : TestBase
    {
        [Fact]
        public void ObjectIdentity()
        {
            var code = "";
            var compilation = CreateCompilation( code );

            Xunit.Assert.Same( compilation.DeclaredTypes, compilation.DeclaredTypes );
        }

        [Fact]
        public void TypeInfos()
        {
            var code = @"
class C
{
    class D { }
}

namespace NS
{
    class C {}
}";

            var compilation = CreateCompilation( code );

            var types = compilation.DeclaredTypes.ToList();
            Xunit.Assert.Equal( 2, types.Count );

            var c1 = types[0];
            Xunit.Assert.Equal( "C", c1.Name );
            Xunit.Assert.Equal( "C", c1.FullName );
            Xunit.Assert.Null( c1.ContainingElement );

            var d = c1.NestedTypes.Single();
            Xunit.Assert.Equal( "D", d.Name );
            Xunit.Assert.Equal( "C.D", d.FullName );
            Xunit.Assert.Same( c1, d.ContainingElement );

            var c2 = types[1];
            Xunit.Assert.Equal( "C", c2.Name );
            Xunit.Assert.Equal( "NS.C", c2.FullName );
            Xunit.Assert.Null( c2.ContainingElement );
        }

        [Fact]
        public void LocalFunctions()
        {
            var code = @"
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

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes.Single();
            Xunit.Assert.Equal( "C", type.Name );

            var methods = type.Methods;

            Xunit.Assert.Single( methods );

            var method = methods[0];
            Xunit.Assert.Equal( "M", method.Name );
            Xunit.Assert.Same( type, method.ContainingElement );

            var outerLocalFunction = method.LocalFunctions.Single();
            Xunit.Assert.Equal( "Outer", outerLocalFunction.Name );
            Xunit.Assert.Same( method, outerLocalFunction.ContainingElement );

            var innerLocalFunction = outerLocalFunction.LocalFunctions.Single();
            Xunit.Assert.Equal( "Inner", innerLocalFunction.Name );
            Xunit.Assert.Same( outerLocalFunction, innerLocalFunction.ContainingElement );
        }

        [Fact]
        public void AttributeData()
        {
            var code = @"
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
            var compilation = CreateCompilation( code );

            var attribute = compilation.DeclaredTypes.ElementAt( 1 ).Attributes.Single();
            Xunit.Assert.Equal( "TestAttribute", attribute.Type.FullName );
            Xunit.Assert.Equal<IReadOnlyList<object>>( expected: new object?[] { 42, "foo", null }, actual: attribute.ConstructorArguments );
            var namedArguments = attribute.NamedArguments;
            Xunit.Assert.Equal( 2, namedArguments.Count );
            Xunit.Assert.Equal<object>( 1, namedArguments.GetByName( "E" ) );
            var types = Xunit.Assert.IsAssignableFrom<IReadOnlyList<object?>>( namedArguments.GetByName( "Types" ) );
            Xunit.Assert.Equal( 3, types.Count );
            var type0 = Xunit.Assert.IsAssignableFrom<INamedType>( types[0] );
            Xunit.Assert.Equal( "E", type0.FullName );
            var type1 = Xunit.Assert.IsAssignableFrom<INamedType>( types[1] );
            Xunit.Assert.Equal( "System.Action<,>", type1.FullName );
            Xunit.Assert.Null( types[2] );
        }

        [Fact]
        public void Parameters()
        {
            var code = @"
using System;

interface I<T>
{
    void M1(Int32 i, T t, dynamic d, in object o, out String s);
    ref readonly int M2();
}";

            var compilation = CreateCompilation( code );

            var methods = compilation.DeclaredTypes.Single().Methods.ToList();
            Xunit.Assert.Equal( 2, methods.Count );

            var m1 = methods[0];
            Xunit.Assert.Equal( "M1", m1.Name );

            CheckParameterData( m1.ReturnParameter!, m1, "void", null, -1 );
            Xunit.Assert.Equal( 5, m1.Parameters.Count );
            CheckParameterData( m1.Parameters[0], m1, "int", "i", 0 );
            CheckParameterData( m1.Parameters[1], m1, "T", "t", 1 );
            CheckParameterData( m1.Parameters[2], m1, "dynamic", "d", 2 );
            CheckParameterData( m1.Parameters[3], m1, "object", "o", 3 );
            CheckParameterData( m1.Parameters[4], m1, "string", "s", 4 );

            var m2 = methods[1];
            Xunit.Assert.Equal( "M2", m2.Name );

            CheckParameterData( m2.ReturnParameter!, m2, "int", null, -1 );
            Xunit.Assert.Equal( 0, m2.Parameters.Count );

            static void CheckParameterData(
                IParameter parameter, ICodeElement containingElement, string typeName, string? name, int index )
            {
                Xunit.Assert.Same( containingElement, parameter.ContainingElement );
                Xunit.Assert.Equal( typeName, parameter.ParameterType.ToString() );

                if ( name != null )
                {
                    Xunit.Assert.Equal( name, parameter.Name );
                }
                else
                {
                    _ = Xunit.Assert.Throws<NotSupportedException>( () => _ = parameter.Name );
                }

                Xunit.Assert.Equal( index, parameter.Index );
            }
        }

        [Fact]
        public void GenericArguments()
        {
            var code = @"
class C<T1, T2>
{
    static C<int, string> GetInstance() => null;
}";

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes.Single();

            Xunit.Assert.Equal( new[] { "T1", "T2" }, type.GenericArguments.Select<IType, string>( t => t.ToString() ) );

            var method = type.Methods.First();

            Xunit.Assert.Equal( "C<int, string>", method.ReturnType.ToString() );
            Xunit.Assert.Equal( new[] { "int", "string" }, ((INamedType) method.ReturnType).GenericArguments.Select<IType, string>( t => t.ToString() ) );
        }

        [Fact]
        public void GlobalAttributes()
        {
            var code = @"
using System;

[module: MyAttribute(""m"")]
[assembly: MyAttribute(""a"")]

class MyAttribute : Attribute
{
    public MyAttribute(string target) {}
}
";

            var compilation = CreateCompilation( code );

            var attributes = compilation.Attributes.ToArray();

            Xunit.Assert.Equal( 2, attributes.Length );

            Xunit.Assert.Equal( "MyAttribute", attributes[0].Type.FullName );
            Xunit.Assert.Equal( "a", Xunit.Assert.Single<object>( attributes[0].ConstructorArguments ) );

            Xunit.Assert.Equal( "MyAttribute", attributes[1].Type.FullName );
            Xunit.Assert.Equal( "m", Xunit.Assert.Single<object>( attributes[1].ConstructorArguments ) );
        }

        [Fact]
        public void Arrays()
        {
            var code = @"
class C
{
    void M(int[] i) {}
}
";

            var compilation = CreateCompilation( code );

            var parameterTypes = from type in compilation.DeclaredTypes
                                 from method in type.Methods
                                 from parameter in method.Parameters
                                 select parameter.ParameterType;
            var parameterType = Xunit.Assert.Single<IType>( parameterTypes )!;

            Xunit.Assert.Equal( "int[]", parameterType.ToString() );
            Xunit.Assert.True( parameterType.Is( typeof( int[] ) ) );
            Xunit.Assert.False( parameterType.Is( typeof( int[,] ) ) );

            var arrayType = Xunit.Assert.IsAssignableFrom<IArrayType>( parameterType );

            Xunit.Assert.Equal( "int", arrayType.ElementType.ToString() );
            Xunit.Assert.True( arrayType.ElementType.Is( typeof( int ) ) );
            Xunit.Assert.Equal( 1, arrayType.Rank );
        }

        [Fact]
        public void Properties()
        {
            var code = @"
class C
{
    int Auto { get; set; }
    int GetOnly { get; }
    int ReadWrite { get => 0; set {} }
    int ReadOnly { get => 0; }
    int WriteOnly { set {} }
    int field;
}";

            var compilation = CreateCompilation( code );

            var type = Xunit.Assert.Single( compilation.DeclaredTypes );

            var propertyNames = type.Properties.Select( p => p.Name );

            Xunit.Assert.Equal( new[] { "Auto", "GetOnly", "ReadWrite", "ReadOnly", "WriteOnly", "field" }, propertyNames );
        }

        [Fact]
        public void RefProperties()
        {
            var code = @"
class C
{
    int field;

    int None { get; set; }
    ref int Ref => ref field;
    ref readonly int RefReadonly => ref field;
}";

            var compilation = CreateCompilation( code );

            var type = Xunit.Assert.Single( compilation.DeclaredTypes );

            var refKinds = type.Properties.Select( p => p.RefKind );

            Xunit.Assert.Equal( new[] { None, None, Ref, RefReadOnly }, refKinds );
        }

        [Fact]
        public void MethodKinds()
        {
            var code = @"
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

            var type = Xunit.Assert.Single( compilation.DeclaredTypes );

            var methodKinds = new[]
            {
                Default,
                Finalizer,
                PropertyGet,
                PropertySet,
                EventAdd,
                EventRemove,
                ExplicitInterfaceImplementation,
                ConversionOperator,
                UserDefinedOperator
            };

            Xunit.Assert.Equal( methodKinds, type.Methods.Select( m => m.MethodKind ) );
            Xunit.Assert.Single( type.Constructors );
            Xunit.Assert.NotNull( type.StaticConstructor );

            Xunit.Assert.Equal( LocalFunction, type.Methods.First().LocalFunctions.Single().MethodKind );
        }

        [Fact]
        public void TypeKinds()
        {
            var code = @"
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

            var type = Xunit.Assert.Single( compilation.DeclaredTypes );

            var typeKinds = new[] { TypeKind.Array, Class, TypeKind.Delegate, Dynamic, TypeKind.Enum, GenericParameter, Interface, Pointer, Struct };

            Xunit.Assert.Equal( typeKinds, type.Properties.Select( p => p.Type.TypeKind ) );
        }

        [Fact]
        public void ParameterKinds()
        {
            var code = @"
class C
{
    int i;

    void M1(int i, in int j, ref int k, out int m) => m = 0;
    ref int M2() => ref i;
    ref readonly int M3 => ref i;
}";

            var compilation = CreateCompilation( code );

            var type = Xunit.Assert.Single( compilation.DeclaredTypes );

            Xunit.Assert.Equal( new[] { None, In, Ref, Out }, type.Methods.First().Parameters.Select( p => p.RefKind ) );
            Xunit.Assert.Equal( new[] { None, Ref, RefReadOnly }, type.Methods.Select( m => m.ReturnParameter.RefKind ) );
        }

        [Fact]
        public void ParameterDefaultValue()
        {
            var code = @"
using System;

class C
{
    void M(int i, int j = 42, string s = ""forty two"", decimal d = 3.14m, DateTime dt = default, DateTime? dt2 = null, object o = null) {}
}";

            var compilation = CreateCompilation( code );

            var type = Xunit.Assert.Single( compilation.DeclaredTypes );

            var method = type.Methods.First();

            var parametersWithoutDefaults = new[] { method.ReturnParameter!, method.Parameters[0] };

            foreach ( var parameter in parametersWithoutDefaults )
            {
                Xunit.Assert.False( parameter.DefaultValue.HasValue );
                Xunit.Assert.Throws<System.InvalidOperationException>( () => parameter.DefaultValue.Value );
            }

            var parametersWithDefaults = method.Parameters.Skip( 1 );

            foreach ( var parameter in parametersWithDefaults )
            {
                Xunit.Assert.True( parameter.DefaultValue.HasValue );
            }

            Xunit.Assert.Equal<object>( new object?[] { 42, "forty two", 3.14m, null, null, null }, parametersWithDefaults.Select( (Func<IParameter, object>) (p => p.DefaultValue.Value) ) );
        }

        [Fact]
        public void GetTypeByReflectionType()
        {
            var compilation = CreateCompilation( null );

            Xunit.Assert.Equal( "System.Collections.Generic.List<T>.Enumerator", compilation.Factory.GetTypeByReflectionType( typeof( List<>.Enumerator ) )!.ToString() );
            Xunit.Assert.Equal( "System.Collections.Generic.Dictionary<int, string>", compilation.Factory.GetTypeByReflectionType( typeof( Dictionary<int, string> ) )!.ToString() );
            Xunit.Assert.Equal( "int[][*,*]", compilation.Factory.GetTypeByReflectionType( typeof( int[][,] ) )!.ToString() );
            Xunit.Assert.Equal( "void*", compilation.Factory.GetTypeByReflectionType( typeof( void* ) )!.ToString() );

            Xunit.Assert.Throws<System.ArgumentException>( () => compilation.Factory.GetTypeByReflectionType( typeof( int ).MakeByRefType() ) );
        }

        [Fact]
        public void TypeName()
        {
            var code = @"
using System.Collections.Generic;

class C<T>
{
    int i;
    List<T>.Enumerator e;
    Dictionary<int, string> d;
    (int i, int j) t;
}";

            var compilation = CreateCompilation( code );

            var type = Xunit.Assert.Single( compilation.DeclaredTypes );

            var fieldTypes = type.Properties.Select( p => (INamedType) p.Type );

            Xunit.Assert.Equal( new[] { "Int32", "Enumerator", "Dictionary", "ValueTuple" }, fieldTypes.Select( t => t.Name ) );
            Xunit.Assert.Equal( new[] { "int", "System.Collections.Generic.List<T>.Enumerator", "System.Collections.Generic.Dictionary<int, string>", "(int i, int j)" }, fieldTypes.Select( t => t.FullName ) );
        }

        [Fact]
        public void PartialType()
        {
            var code = @"
using System.Collections.Generic;

class A
{
}

partial class B
{
}
";

            var compilation = CreateCompilation( code );

            Xunit.Assert.Equal( 2, compilation.DeclaredTypes.Count );

            Xunit.Assert.False( compilation.DeclaredTypes.Single( t => t.Name == "A" ).IsPartial );
            Xunit.Assert.True( compilation.DeclaredTypes.Single( t => t.Name == "B" ).IsPartial );
        }

        [Fact]
        public void WithGenericArguments()
        {
            var code = @"
class C<TC>
{
    (TC, TM) M<TM>() => default;
}
";

            var compilation = CreateCompilation( code );

            var type = Xunit.Assert.Single( compilation.DeclaredTypes );

            var intType = compilation.Factory.GetTypeByReflectionType( typeof( int ) )!;
            var stringType = compilation.Factory.GetTypeByReflectionType( typeof( string ) )!;

            var openTypeMethod = type.Methods.First();
            var closedTypeMethod = type.WithGenericArguments( stringType ).Methods.First();

            Xunit.Assert.Equal( "(TC, TM)", openTypeMethod.ReturnType.ToString() );
            Xunit.Assert.Equal( "(TC, int)", openTypeMethod.WithGenericArguments( intType ).ReturnType.ToString() );
            Xunit.Assert.Equal( "(string, TM)", closedTypeMethod.ReturnType.ToString() );
            Xunit.Assert.Equal( "(string, int)", closedTypeMethod.WithGenericArguments( intType ).ReturnType.ToString() );
        }
    }
}
