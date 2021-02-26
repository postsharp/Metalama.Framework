// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Xunit;
using static Caravela.Framework.Code.MethodKind;
using static Caravela.Framework.Code.RefKind;
using static Caravela.Framework.Code.TypeKind;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Caravela.Framework.UnitTests
{
    public class CodeModelTests : TestBase
    {
        [Fact]
        public void ObjectIdentity()
        {
            var code = "";
            var compilation = CreateCompilation( code );

            Assert.Same( compilation.DeclaredTypes, compilation.DeclaredTypes );
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
            Assert.Equal( 2, types.Count );

            var c1 = types[0];
            Assert.Equal( "C", c1.Name );
            Assert.Equal( "C", c1.FullName );
            Assert.IsAssignableFrom<ICompilation>( c1.ContainingElement );

            var d = c1.NestedTypes.Single();
            Assert.Equal( "D", d.Name );
            Assert.Equal( "C.D", d.FullName );
            Assert.Same( c1, d.ContainingElement );

            var c2 = types[1];
            Assert.Equal( "C", c2.Name );
            Assert.Equal( "NS.C", c2.FullName );
            Assert.IsAssignableFrom<ICompilation>( c2.ContainingElement );
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
            Assert.Equal( "C", type.Name );

            var methods = type.Methods;

            Assert.Single( methods );

            var method = methods[0];
            Assert.Equal( "M", method.Name );
            Assert.Same( type, method.ContainingElement );

            var outerLocalFunction = method.LocalFunctions.Single();
            Assert.Equal( "Outer", outerLocalFunction.Name );
            Assert.Same( method, outerLocalFunction.ContainingElement );

            var innerLocalFunction = outerLocalFunction.LocalFunctions.Single();
            Assert.Equal( "Inner", innerLocalFunction.Name );

            Assert.Same( outerLocalFunction, innerLocalFunction.ContainingElement );
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

[Test(42, ""foo"", null, E = E.G, Types = new[] { typeof(E), typeof(Action<,>), null, typeof(Action<E>), typeof(E*) })]
class TestAttribute : Attribute
{
    public TestAttribute(int i, string s, object o) {}

    public E E { get; set; }
    public Type[] Types { get; set; }
}";
            var compilation = CreateCompilation( code );

            var attribute = compilation.DeclaredTypes.ElementAt( 1 ).Attributes.Single();
            Assert.Equal( "TestAttribute", attribute.Type.FullName );
            Assert.Equal( new object?[] { 42, "foo", null }, attribute.ConstructorArguments.Select( a => a.Value ) );
            var namedArguments = attribute.NamedArguments;
            Assert.Equal( 2, namedArguments.Count );
            Assert.Equal( 1, namedArguments.GetValue( "E" ) );
            var types = Assert.IsAssignableFrom<IReadOnlyList<TypedConstant>>( namedArguments.GetValue( "Types" ) );
            Assert.Equal( 5, types.Count );
            var type0 = Assert.IsAssignableFrom<INamedType>( types[0].Value );
            Assert.Equal( "E", type0.FullName );
            var type1 = Assert.IsAssignableFrom<INamedType>( types[1].Value );
            Assert.Equal( "System.Action<,>", type1.FullName );
            Assert.Null( types[2].Value );
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
            Assert.Equal( 2, methods.Count );

            var m1 = methods[0];
            Assert.Equal( "M1", m1.Name );

            CheckParameterData( m1.ReturnParameter!, m1, "void", null, -1 );
            Assert.Equal( 5, m1.Parameters.Count );
            CheckParameterData( m1.Parameters[0], m1, "int", "i", 0 );
            CheckParameterData( m1.Parameters[1], m1, "T", "t", 1 );
            CheckParameterData( m1.Parameters[2], m1, "dynamic", "d", 2 );
            CheckParameterData( m1.Parameters[3], m1, "object", "o", 3 );
            CheckParameterData( m1.Parameters[4], m1, "string", "s", 4 );

            var m2 = methods[1];
            Assert.Equal( "M2", m2.Name );

            CheckParameterData( m2.ReturnParameter!, m2, "int", null, -1 );
            Assert.Equal( 0, m2.Parameters.Count );

            static void CheckParameterData(
                IParameter parameter, ICodeElement containingElement, string typeName, string? name, int index )
            {
                Assert.Same( containingElement, parameter.ContainingElement );
                Assert.Equal( typeName, parameter.ParameterType.ToString() );

                if ( name != null )
                {
                    Assert.Equal( name, parameter.Name );
                }
                else
                {
                    _ = Assert.Throws<NotSupportedException>( () => _ = parameter.Name );
                }

                Assert.Equal( index, parameter.Index );
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

            Assert.Equal( new[] { "T1", "T2" }, type.GenericArguments.Select<IType, string>( t => t!.ToString()! ) );

            var method = type.Methods.First();

            Assert.Equal( "C<int, string>", method.ReturnType.ToString() );
            Assert.Equal( new[] { "int", "string" }, ((INamedType) method.ReturnType).GenericArguments.Select( t => t.ToString() ) );
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

            Assert.Equal( 2, attributes.Length );

            Assert.Equal( "MyAttribute", attributes[0].Type.FullName );
            Assert.Equal( "a", Assert.Single( attributes[0].ConstructorArguments.Select( a => a.Value ) ) );

            Assert.Equal( "MyAttribute", attributes[1].Type.FullName );
            Assert.Equal( "m", Assert.Single( attributes[1].ConstructorArguments.Select( a => a.Value ) ) );
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
            var parameterType = Assert.Single( parameterTypes )!;

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

            var type = Assert.Single( compilation.DeclaredTypes )!;

            var propertyNames = type.Properties.Select( p => p.Name );

            Assert.Equal( new[] { "Auto", "GetOnly", "ReadWrite", "ReadOnly", "WriteOnly", "field" }, propertyNames );
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

            var type = Assert.Single( compilation.DeclaredTypes )!;

            var refKinds = type.Properties.Select( p => p.RefKind );

            Assert.Equal( new[] { None, None, Ref, RefReadOnly }, refKinds );
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

            var type = Assert.Single( compilation.DeclaredTypes )!;

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

            Assert.Equal( methodKinds, type.Methods.Select( m => m.MethodKind ) );
            Assert.Single( type.Constructors );
            Assert.NotNull( type.StaticConstructor );

            Assert.Equal( LocalFunction, type.Methods.First().LocalFunctions.Single().MethodKind );
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

            var type = Assert.Single( compilation.DeclaredTypes )!;

            var typeKinds = new[] { TypeKind.Array, Class, TypeKind.Delegate, Dynamic, TypeKind.Enum, GenericParameter, Interface, Pointer, Struct };

            Assert.Equal( typeKinds, type.Properties.Select( p => p.Type.TypeKind ) );
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

            var type = Assert.Single( compilation.DeclaredTypes )!;

            Assert.Equal( new[] { None, In, Ref, Out }, type.Methods.First().Parameters.Select( p => p.RefKind ) );
            Assert.Equal( new[] { None, Ref, RefReadOnly }, type.Methods.Select( m => m.ReturnParameter.RefKind ) );
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

            var type = Assert.Single( compilation.DeclaredTypes )!;

            var method = type.Methods.First();

            var parametersWithoutDefaults = new[] { method.ReturnParameter!, method.Parameters[0] };

            foreach ( var parameter in parametersWithoutDefaults )
            {
                Assert.False( parameter.DefaultValue.IsAssigned );
                Assert.Throws<InvalidOperationException>( () => parameter.DefaultValue.Value );
            }

            var parametersWithDefaults = method.Parameters.Skip( 1 );

            foreach ( var parameter in parametersWithDefaults )
            {
                Assert.True( parameter.DefaultValue.IsAssigned );
            }

            Assert.Equal( new object?[] { 42, "forty two", 3.14m, null, null, null }, parametersWithDefaults.Select( p => p.DefaultValue.Value ) );
        }

        [Fact]
        public void GetTypeByReflectionType()
        {
            var compilation = CreateCompilation( null );

            Assert.Equal( "System.Collections.Generic.List<T>.Enumerator", compilation.Factory.GetTypeByReflectionType( typeof( List<>.Enumerator ) )!.ToString() );
            Assert.Equal( "System.Collections.Generic.Dictionary<int, string>", compilation.Factory.GetTypeByReflectionType( typeof( Dictionary<int, string> ) )!.ToString() );
            Assert.Equal( "int[][*,*]", compilation.Factory.GetTypeByReflectionType( typeof( int[][,] ) )!.ToString() );
            Assert.Equal( "void*", compilation.Factory.GetTypeByReflectionType( typeof( void* ) )!.ToString() );

            Assert.Throws<ArgumentException>( () => compilation.Factory.GetTypeByReflectionType( typeof( int ).MakeByRefType() ) );
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

            var type = Assert.Single( compilation.DeclaredTypes )!;

            var fieldTypes = type.Properties.Select( p => (INamedType) p.Type );

            Assert.Equal( new[] { "Int32", "Enumerator", "Dictionary", "ValueTuple" }, fieldTypes.Select( t => t.Name ) );
            Assert.Equal( new[] { "int", "System.Collections.Generic.List<T>.Enumerator", "System.Collections.Generic.Dictionary<int, string>", "(int i, int j)" }, fieldTypes.Select( t => t.FullName ) );
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

            Assert.Equal( 2, compilation.DeclaredTypes.Count );

            Assert.False( compilation.DeclaredTypes.Single( t => t.Name == "A" ).IsPartial );
            Assert.True( compilation.DeclaredTypes.Single( t => t.Name == "B" ).IsPartial );
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

            var type = Assert.Single( compilation.DeclaredTypes )!;

            var intType = compilation.Factory.GetTypeByReflectionType( typeof( int ) )!;
            var stringType = compilation.Factory.GetTypeByReflectionType( typeof( string ) )!;

            var openTypeMethod = type.Methods.First();
            var closedTypeMethod = type.WithGenericArguments( stringType ).Methods.First();

            Assert.Equal( "(TC, TM)", openTypeMethod.ReturnType.ToString() );
            Assert.Equal( "(TC, int)", openTypeMethod.WithGenericArguments( intType ).ReturnType.ToString() );
            Assert.Equal( "(string, TM)", closedTypeMethod.ReturnType.ToString() );
            Assert.Equal( "(string, int)", closedTypeMethod.WithGenericArguments( intType ).ReturnType.ToString() );
        }

        [Fact]
        public void Depth()
        {
            var code = @"
class C 
{
    void M() { void N() {} }

    class D : C, L
    {
    }
}

interface I {}
interface J : I {}
interface K : I {}
interface L : J, K {}
";
            
            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes.OfName( "C" ).Single();

            Assert.Equal( 3, compilation.GetDepth( type ) );
            Assert.Equal( 4, compilation.GetDepth( type.Methods.OfName( "M" ).Single() ) );
            Assert.Equal( 5, compilation.GetDepth( type.Methods.OfName( "M" ).Single().LocalFunctions.Single() ) );
            Assert.Equal( 1, compilation.GetDepth( compilation.DeclaredTypes.OfName( "I" ).Single() ) );
            Assert.Equal( 2, compilation.GetDepth( compilation.DeclaredTypes.OfName( "J" ).Single() ) );
            Assert.Equal( 2, compilation.GetDepth( compilation.DeclaredTypes.OfName( "K" ).Single() ) );
            Assert.Equal( 3, compilation.GetDepth( compilation.DeclaredTypes.OfName( "L" ).Single() ) );
            Assert.Equal( 4, compilation.GetDepth( type.NestedTypes.OfName( "D" ).Single() ) );
        }
    }
}
