// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Caravela.Framework.Code.MethodKind;
using static Caravela.Framework.Code.RefKind;
using static Caravela.Framework.Code.TypeKind;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Caravela.Framework.Tests.UnitTests.CodeModel
{
    public class CodeModelTests : TestBase
    {
        [Fact]
        public void ObjectIdentity()
        {
            // This basically tests that [Memo] works.

            var code = "";
            var compilation = this.CreateCompilationModel( code );

            var types1 = compilation.Types;
            var types2 = compilation.Types;

            Assert.Same( types1, types2 );
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

            var compilation = this.CreateCompilationModel( code );

            var types = compilation.Types.ToList();
            Assert.Equal( 2, types.Count );

            var c1 = types[0];
            Assert.Equal( "C", c1.Name );
            Assert.Equal( "C", c1.FullName );
            Assert.IsAssignableFrom<ICompilation>( c1.ContainingDeclaration );

            var d = c1.NestedTypes.Single();
            Assert.Equal( "D", d.Name );
            Assert.Equal( "C.D", d.FullName );
            Assert.Same( c1, d.ContainingDeclaration );

            var c2 = types[1];
            Assert.Equal( "C", c2.Name );
            Assert.Equal( "NS.C", c2.FullName );
            Assert.IsAssignableFrom<ICompilation>( c2.ContainingDeclaration );
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

            var compilation = this.CreateCompilationModel( code );

            var type = compilation.Types.Single();
            Assert.Equal( "C", type.Name );

            var methods = type.Methods;

            Assert.Single( methods );

            var method = methods[0];
            Assert.Equal( "M", method.Name );
            Assert.Same( type, method.ContainingDeclaration );

            var outerLocalFunction = method.LocalFunctions.Single();
            Assert.Equal( "Outer", outerLocalFunction.Name );
            Assert.Same( method, outerLocalFunction.ContainingDeclaration );

            var innerLocalFunction = outerLocalFunction.LocalFunctions.Single();
            Assert.Equal( "Inner", innerLocalFunction.Name );

            Assert.Same( outerLocalFunction, innerLocalFunction.ContainingDeclaration );
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

            var compilation = this.CreateCompilationModel( code );

            var attribute = compilation.Types.ElementAt( 1 ).Attributes.Single();
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

            var compilation = this.CreateCompilationModel( code );

            var methods = compilation.Types.Single().Methods.ToList();
            Assert.Equal( 2, methods.Count );

            var m1 = methods[0];
            Assert.Equal( "M1", m1.Name );

            Assert.True( m1.ReturnParameter.IsReturnParameter );
            Assert.False( m1.Parameters[0].IsReturnParameter );

            Assert.Equal( 0, m1.Parameters.OfParameterType( typeof(int) ).First().Index );
            Assert.Equal( 0, m1.Parameters.OfParameterType<int>().First().Index );
            Assert.Equal( 0, m1.Parameters.OfParameterType( compilation.Factory.GetSpecialType( SpecialType.Int32 ) ).First().Index );

            CheckParameterData( m1.ReturnParameter, m1, "void", null, -1 );
            Assert.Equal( 5, m1.Parameters.Count );
            CheckParameterData( m1.Parameters[0], m1, "int", "i", 0 );
            CheckParameterData( m1.Parameters[1], m1, "T", "t", 1 );
            CheckParameterData( m1.Parameters[2], m1, "dynamic", "d", 2 );
            CheckParameterData( m1.Parameters[3], m1, "object", "o", 3 );
            CheckParameterData( m1.Parameters[4], m1, "string", "s", 4 );

            var m2 = methods[1];
            Assert.Equal( "M2", m2.Name );

            CheckParameterData( m2.ReturnParameter, m2, "int", null, -1 );
            Assert.Equal( 0, m2.Parameters.Count );

            static void CheckParameterData( IParameter parameter, IDeclaration containingDeclaration, string typeName, string? name, int index )
            {
                Assert.Same( containingDeclaration, parameter.ContainingDeclaration );
                Assert.Equal( typeName, parameter.Type.ToString() );

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

            var compilation = this.CreateCompilationModel( code );

            var type = compilation.Types.Single();

            Assert.Equal( new[] { "T1", "T2" }, type.TypeArguments.Select<IType, string>( t => t.ToString()! ) );

            var method = type.Methods.First();

            Assert.Equal( "C<int, string>", method.ReturnType.ToString() );
            Assert.Equal( new[] { "int", "string" }, ((INamedType) method.ReturnType).TypeArguments.Select( t => t.ToString() ) );
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

            var compilation = this.CreateCompilationModel( code );

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

            var compilation = this.CreateCompilationModel( code );

            var parameterTypes = from type in compilation.Types
                                 from method in type.Methods
                                 from parameter in method.Parameters
                                 select parameter.Type;

            var parameterType = Assert.Single( parameterTypes )!;

            Assert.Equal( "int[]", parameterType.ToString() );
            Assert.True( parameterType.Is( typeof(int[]) ) );
            Assert.False( parameterType.Is( typeof(int[,]) ) );

            var arrayType = Assert.IsAssignableFrom<IArrayType>( parameterType );

            Assert.Equal( "int", arrayType.ElementType.ToString() );
            Assert.True( arrayType.ElementType.Is( typeof(int) ) );
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

            var compilation = this.CreateCompilationModel( code );

            var type = Assert.Single( compilation.Types )!;

            var propertyNames = type.Properties.Select( p => p.Name );

            Assert.Equal( new[] { "Auto", "GetOnly", "ReadWrite", "ReadOnly", "WriteOnly" }, propertyNames );
        }

        [Fact]
        public void Fields()
        {
            var code = @"
class C
{
    int a = 0, b;
    int c;    
    int AutoProperty { get; set; }
    event Handler EventField;

    delegate void Handler();
}";

            var compilation = this.CreateCompilationModel( code );

            var type = Assert.Single( compilation.Types )!;

            var fieldNames = type.Fields.Select( p => p.Name );

            Assert.Equal( new[] { "a", "b", "c" }, fieldNames );
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

            var compilation = this.CreateCompilationModel( code );

            var type = Assert.Single( compilation.Types )!;

            var refKinds = type.Properties.Select( p => p.RefKind );

            Assert.Equal( new[] { None, Ref, RefReadOnly }, refKinds );
        }

        [Fact]
        public void Events()
        {
            var code = @"
class C
{
    event Handler Event
    {
        add {}
        remove {}
    }

    event Handler EventField;

    delegate void Handler();
}";

            var compilation = this.CreateCompilationModel( code );

            var type = Assert.Single( compilation.Types )!;

            var eventNames = type.Events.Select( p => p.Name );

            Assert.Equal( new[] { "Event", "EventField" }, eventNames );
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

            var compilation = this.CreateCompilationModel( code );

            var type = Assert.Single( compilation.Types )!;

            Assert.Equal(
                new[] { Default, Finalizer, ExplicitInterfaceImplementation, ConversionOperator, UserDefinedOperator },
                type.Methods.Select( m => m.MethodKind ) );

            Assert.Equal( new[] { PropertyGet, PropertySet }, type.Properties.SelectMany( p => new[] { p.GetMethod!.MethodKind, p.SetMethod!.MethodKind } ) );
            Assert.Equal( new[] { EventAdd, EventRemove }, type.Events.SelectMany( p => new[] { p.AddMethod.MethodKind, p.RemoveMethod.MethodKind } ) );
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

            var compilation = this.CreateCompilationModel( code );

            var type = Assert.Single( compilation.Types )!;

            var typeKinds = new[] { TypeKind.Array, Class, TypeKind.Delegate, Dynamic, TypeKind.Enum, GenericParameter, Interface, Pointer, Struct };

            Assert.Equal( typeKinds, type.Fields.Select( p => p.Type.TypeKind ) );
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
    ref readonly int M3() => ref i;
}";

            var compilation = this.CreateCompilationModel( code );

            var type = Assert.Single( compilation.Types )!;

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

            var compilation = this.CreateCompilationModel( code );

            var type = Assert.Single( compilation.Types )!;

            var method = type.Methods.First();

            var parametersWithoutDefaults = new[] { method.ReturnParameter, method.Parameters[0] };

            foreach ( var parameter in parametersWithoutDefaults )
            {
                Assert.False( parameter.DefaultValue.IsAssigned );
                _ = Assert.Throws<ArgumentNullException>( () => parameter.DefaultValue.Value );
            }

            var parametersWithDefaults = method.Parameters.Skip( 1 ).ToList();

            foreach ( var parameter in parametersWithDefaults )
            {
                Assert.True( parameter.DefaultValue.IsAssigned );
            }

            Assert.Equal( new object?[] { 42, "forty two", 3.14m, null, null, null }, parametersWithDefaults.Select( p => p.DefaultValue.Value ) );
        }

        [Fact]
        public void GetTypeByReflectionType()
        {
            var compilation = this.CreateCompilationModel( "" );

            Assert.Equal(
                "System.Collections.Generic.List<T>.Enumerator",
                compilation.Factory.GetTypeByReflectionType( typeof(List<>.Enumerator) ).ToString() );

            Assert.Equal(
                "System.Collections.Generic.Dictionary<int, string>",
                compilation.Factory.GetTypeByReflectionType( typeof(Dictionary<int, string>) ).ToString() );

            Assert.Equal( "int[][*,*]", compilation.Factory.GetTypeByReflectionType( typeof(int[][,]) ).ToString() );
            Assert.Equal( "void*", compilation.Factory.GetTypeByReflectionType( typeof(void*) ).ToString() );

            Assert.Throws<ArgumentException>( () => compilation.Factory.GetTypeByReflectionType( typeof(int).MakeByRefType() ) );
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

            var compilation = this.CreateCompilationModel( code );

            var type = Assert.Single( compilation.Types )!;

            var fieldTypes = type.Fields.Select( p => (INamedType) p.Type ).ToList();

            Assert.Equal( new[] { "Int32", "Enumerator", "Dictionary", "ValueTuple" }, fieldTypes.Select( t => t.Name ) );

            Assert.Equal(
                new[] { "int", "System.Collections.Generic.List<T>.Enumerator", "System.Collections.Generic.Dictionary<int, string>", "(int i, int j)" },
                fieldTypes.Select( t => t.FullName ) );
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

            var compilation = this.CreateCompilationModel( code );

            Assert.Equal( 2, compilation.Types.Count );

            Assert.False( compilation.Types.Single( t => t.Name == "A" ).IsPartial );
            Assert.True( compilation.Types.Single( t => t.Name == "B" ).IsPartial );
        }

        [Fact]
        public void ConstructGenericInstance()
        {
            var code = @"
class C<TC>
{
    (TC, TM) M<TM>() => default;
}
";

            var compilation = this.CreateCompilationModel( code );

            var type = Assert.Single( compilation.Types )!;
            Assert.True( type.IsOpenGeneric );

            var openTypeMethod = type.Methods.First();
            Assert.True( openTypeMethod.IsOpenGeneric );
            var closedType = type.ConstructGenericInstance( typeof(string) );
            Assert.False( closedType.IsOpenGeneric );
            var closedTypeMethod = closedType.Methods.First();
            Assert.True( closedTypeMethod.IsOpenGeneric );
            var closedMethod = closedTypeMethod.ConstructGenericInstance( typeof(int) );
            Assert.False( closedMethod.IsOpenGeneric );

            Assert.Equal( "(TC, TM)", openTypeMethod.ReturnType.ToString() );
            Assert.Throws<InvalidOperationException>( () => openTypeMethod.ConstructGenericInstance( typeof(int) ) );
            Assert.Equal( "(string, TM)", closedTypeMethod.ReturnType.ToString() );
            Assert.Equal( "(string, int)", closedMethod.ReturnType.ToString() );
        }

        [Fact]
        public void WithinGenericTypeInstance()
        {
            var code = @"
class Class<T>
{
    T field;
    T Property { get; set; }
    event System.Action<T> Event;
    T Method() => default;
    Class( T a ) {}
    static Class() {}
}
";

            var compilation = this.CreateCompilationModel( code );

            var openType = compilation.Types.Single();
            var typeInstance = openType.ConstructGenericInstance( typeof(string) );

            Assert.Equal( "string", openType.Fields.Single().ForTypeInstance( typeInstance ).Type.ToString() );
            Assert.Equal( "string", openType.Properties.Single().ForTypeInstance( typeInstance ).Type.ToString() );
            Assert.Equal( "System.Action<string>", openType.Events.Single().ForTypeInstance( typeInstance ).Type.ToString() );
            Assert.Equal( "string", openType.Methods.Single().ForTypeInstance( typeInstance ).ReturnType.ToString() );
            Assert.Equal( "string", openType.Constructors.Single().ForTypeInstance( typeInstance ).Parameters[0].Type.ToString() );
            Assert.Equal( typeInstance, openType.StaticConstructor!.ForTypeInstance( typeInstance ).DeclaringType );
        }

        [Fact]
        public void ConstructGenericInstanceNestedType()
        {
            var code = @"
class Parent<TParent>
{
    class NestedGeneric<TNested>
    {
        (TParent, TNested, TMethod) GenericMethod<TMethod>() => default;
        (TParent, TNested) NonGenericMethod() => default;
    }

    class NestedNonGeneric 
    {
        (TParent, TMethod) GenericMethod<TMethod>() => default;
        TParent NonGenericMethod() => default;
    }
}
";

            var compilation = this.CreateCompilationModel( code );

            // Find the different types and check the IsGeneric and IsOpenGeneric properties.
            var openParentType = Assert.Single( compilation.Types )!;

            var genericNestedTypeOnOpenGenericParent = openParentType.NestedTypes.OfName( "NestedGeneric" ).Single();
            Assert.True( genericNestedTypeOnOpenGenericParent.IsOpenGeneric );
            var genericMethodOnOpenGenericNestedType = genericNestedTypeOnOpenGenericParent.Methods.OfName( "GenericMethod" ).Single();
            Assert.True( genericMethodOnOpenGenericNestedType.IsGeneric );
            Assert.True( genericMethodOnOpenGenericNestedType.IsOpenGeneric );
            var nonGenericMethodOnOpenGenericNestedType = genericNestedTypeOnOpenGenericParent.Methods.OfName( "NonGenericMethod" ).Single();
            Assert.False( nonGenericMethodOnOpenGenericNestedType.IsGeneric );
            Assert.True( nonGenericMethodOnOpenGenericNestedType.IsOpenGeneric );

            var nonGenericNestedTypeOnOpenGenericParent = openParentType.NestedTypes.OfName( "NestedNonGeneric" ).Single();
            Assert.True( nonGenericNestedTypeOnOpenGenericParent.IsOpenGeneric );
            var genericMethodOnOpenNonGenericNestedType = genericNestedTypeOnOpenGenericParent.Methods.OfName( "GenericMethod" ).Single();
            Assert.True( genericMethodOnOpenNonGenericNestedType.IsGeneric );
            Assert.True( genericMethodOnOpenNonGenericNestedType.IsOpenGeneric );
            var nonGenericMethodOnOpenNonGenericNestedType = genericNestedTypeOnOpenGenericParent.Methods.OfName( "NonGenericMethod" ).Single();
            Assert.False( nonGenericMethodOnOpenNonGenericNestedType.IsGeneric );
            Assert.True( nonGenericMethodOnOpenNonGenericNestedType.IsOpenGeneric );

            // Attempt to create a generic instance of a nested type should fail when the parent type is an open generic.
            Assert.Throws<InvalidOperationException>( () => genericNestedTypeOnOpenGenericParent.ConstructGenericInstance( typeof(int) ) );

            // Creating a closed nested type.
            var closedParentType = openParentType.ConstructGenericInstance( typeof(string) );
            var closedGenericNestedType = closedParentType.NestedTypes.OfName( "NestedGeneric" ).Single().ConstructGenericInstance( typeof(int) );
            Assert.Equal( "int", closedGenericNestedType.TypeArguments[0].ToString() );
            Assert.False( closedGenericNestedType.IsOpenGeneric );

            // Open method of closed nested type.
            var openMethodOfClosedNestedType = closedGenericNestedType.Methods.OfName( "GenericMethod" ).Single();

            Assert.Equal( "(string, int, TMethod)", openMethodOfClosedNestedType.ReturnType.ToString() );

            // Closed method in closed nested type.
            var closedMethod = openMethodOfClosedNestedType.ConstructGenericInstance( typeof(long) );
            Assert.Equal( "(string, int, long)", closedMethod.ReturnType.ToString() );
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

            var compilation = this.CreateCompilationModel( code );

            var type = compilation.Types.OfName( "C" ).Single();

            Assert.Equal( 3, compilation.GetDepth( type ) );
            Assert.Equal( 4, compilation.GetDepth( type.Methods.OfName( "M" ).Single() ) );
            Assert.Equal( 5, compilation.GetDepth( type.Methods.OfName( "M" ).Single().LocalFunctions.Single() ) );
            Assert.Equal( 1, compilation.GetDepth( compilation.Types.OfName( "I" ).Single() ) );
            Assert.Equal( 2, compilation.GetDepth( compilation.Types.OfName( "J" ).Single() ) );
            Assert.Equal( 2, compilation.GetDepth( compilation.Types.OfName( "K" ).Single() ) );
            Assert.Equal( 3, compilation.GetDepth( compilation.Types.OfName( "L" ).Single() ) );
            Assert.Equal( 4, compilation.GetDepth( type.NestedTypes.OfName( "D" ).Single() ) );
        }

        [Fact]
        public void CompileTimeOnlyTypesAreInvisible()
        {
            var code = @"
using Caravela.Framework.Aspects;

[CompileTimeOnly]
class C { }

class D 
{
    [CompileTimeOnly]
    class E { }
}
";

            var compilation = this.CreateCompilationModel( code );
            Assert.Single( compilation.Types );
            Assert.Empty( compilation.Types.Single().NestedTypes );
        }
    }
}