// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.AdviceImpl.InterfaceImplementation;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel;

public sealed partial class CodeModelUpdateTests
{
    [Fact]
    public void AddMethodToEmptyIntroducedType_InitializeBefore_Complete()
    {
        using var testContext = this.CreateTestContext();

        const string code = @"
class C
{    
}";

        var immutableCompilation1 = testContext.CreateCompilationModel( code );
        Assert.Empty( immutableCompilation1.Types.Single().Methods );

        var mutableCompilation1 = immutableCompilation1.CreateMutableClone();

        var type1 = Assert.Single( mutableCompilation1.Types );

        // Add a type.
        var typeBuilder = new NamedTypeBuilder( null!, type1, "T" );
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var nestedType = Assert.Single( type2.Types );

        // Instantiate the memoize the collection of methods of the nested type.
        Assert.Empty( nestedType.Methods );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( nestedType.Methods );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().Types.Single().Methods );
    }

    [Fact]
    public void AddMethodToEmptyIntroducedType_InitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        const string code = @"
class C
{    
}";

        var immutableCompilation1 = testContext.CreateCompilationModel( code );
        Assert.Empty( immutableCompilation1.Types.Single().Methods );

        var mutableCompilation1 = immutableCompilation1.CreateMutableClone();

        var type1 = Assert.Single( mutableCompilation1.Types );

        // Add a type.
        var typeBuilder = new NamedTypeBuilder( null!, type1, "T" );
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var nestedType = Assert.Single( type2.Types );

        // Instantiate the memoize the collection of methods of the nested type.
        Assert.Empty( nestedType.Methods.OfName( "M" ) );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( nestedType.Methods.OfName( "M" ) );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().Types.Single().Methods.OfName( "M" ) );
    }

    [Fact]
    public void AddMethodToEmptyIntroducedType_DontInitializeBefore_Complete()
    {
        using var testContext = this.CreateTestContext();

        const string code = @"
class C
{    
}";

        var immutableCompilation1 = testContext.CreateCompilationModel( code );
        Assert.Empty( immutableCompilation1.Types.Single().Methods );

        var mutableCompilation1 = immutableCompilation1.CreateMutableClone();

        var type1 = Assert.Single( mutableCompilation1.Types );

        // Add a type.
        var typeBuilder = new NamedTypeBuilder( null!, type1, "T" );
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var nestedType = Assert.Single( type2.Types );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( nestedType.Methods );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().Types.Single().Methods );
    }

    [Fact]
    public void AddMethodToEmptyIntroducedType_DontInitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        const string code = @"
class C
{    
}";

        var immutableCompilation1 = testContext.CreateCompilationModel( code );
        Assert.Empty( immutableCompilation1.Types.Single().Methods );

        var mutableCompilation1 = immutableCompilation1.CreateMutableClone();

        var type1 = Assert.Single( mutableCompilation1.Types );

        // Add a type.
        var typeBuilder = new NamedTypeBuilder( null!, type1, "T" );
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var nestedType = Assert.Single( type2.Types );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( nestedType.Methods.OfName( "M" ) );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().Types.Single().Methods.OfName( "M" ) );
    }

    [Fact]
    public void AddMethodToBaseTypeOfIntroducedType_InitializeBefore_Complete()
    {
        using var testContext = this.CreateTestContext();

        const string code = @"
class C
{ 
    class B
    {
    }
}";

        var immutableCompilation1 = testContext.CreateCompilationModel( code );
        Assert.Empty( immutableCompilation1.Types.Single().Methods );

        var mutableCompilation1 = immutableCompilation1.CreateMutableClone();

        var type1 = Assert.Single( mutableCompilation1.Types );

        // Add a type.
        var typeBuilder = new NamedTypeBuilder( null!, type1, "T" ) { BaseType = type1.Types.Single() };
        var originalBaseMethodCount = type1.Types.Single().AllMethods.Count;
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var baseType = Assert.Single( type2.Types.OfName( "B" ) );
        var introducedType = Assert.Single( type2.Types.OfName( "T" ) );

        // Instantiate the memoize the collection of methods.
        Assert.Empty( baseType.Methods );
        Assert.Equal( originalBaseMethodCount, introducedType.AllMethods.Count );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, baseType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( baseType.Methods );
        Assert.Equal( originalBaseMethodCount + 1, introducedType.AllMethods.Count );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().Types.OfName( "B" ).Single().Methods );
        Assert.Equal( originalBaseMethodCount, immutableCompilation2.Types.Single().Types.OfName( "T" ).Single().AllMethods.Count );
    }

    [Fact]
    public void AddMethodToBaseTypeOfIntroducedType_InitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        const string code = @"
class C
{ 
    class B
    {
    }
}";

        var immutableCompilation1 = testContext.CreateCompilationModel( code );
        Assert.Empty( immutableCompilation1.Types.Single().Methods );

        var mutableCompilation1 = immutableCompilation1.CreateMutableClone();

        var type1 = Assert.Single( mutableCompilation1.Types );

        // Add a type.
        var typeBuilder = new NamedTypeBuilder( null!, type1, "T" ) { BaseType = type1.Types.Single() };
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var baseType = Assert.Single( type2.Types.OfName( "B" ) );
        var introducedType = Assert.Single( type2.Types.OfName( "T" ) );

        // Instantiate the memoize the collection of methods of the nested type.
        Assert.Empty( baseType.Methods.OfName( "M" ) );
        Assert.Empty( introducedType.AllMethods.OfName( "M" ) );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, baseType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( baseType.Methods.OfName( "M" ) );
        Assert.Single( introducedType.AllMethods.OfName( "M" ) );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().Types.OfName( "B" ).Single().Methods.OfName( "M" ) );
        Assert.Empty( immutableCompilation2.Types.Single().Types.OfName( "T" ).Single().AllMethods.OfName( "M" ) );
    }

    [Fact]
    public void AddMethodToBaseTypeOfIntroducedType_DontInitializeBefore_Complete()
    {
        using var testContext = this.CreateTestContext();

        const string code = @"
class C
{ 
    class B
    {
    }
}";

        var immutableCompilation1 = testContext.CreateCompilationModel( code );
        Assert.Empty( immutableCompilation1.Types.Single().Methods );

        var mutableCompilation1 = immutableCompilation1.CreateMutableClone();

        var type1 = Assert.Single( mutableCompilation1.Types );

        // Add a type.
        var typeBuilder = new NamedTypeBuilder( null!, type1, "T" ) { BaseType = type1.Types.Single() };
        var originalBaseMethodCount = type1.Types.Single().AllMethods.Count;
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var baseType = Assert.Single( type2.Types.OfName( "B" ) );
        var introducedType = Assert.Single( type2.Types.OfName( "T" ) );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, baseType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( baseType.Methods );
        Assert.Equal( originalBaseMethodCount + 1, introducedType.AllMethods.Count );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().Types.OfName( "B" ).Single().Methods );
        Assert.Equal( originalBaseMethodCount, immutableCompilation2.Types.Single().Types.OfName( "T" ).Single().AllMethods.Count );
    }

    [Fact]
    public void AddMethodToBaseTypeOfIntroducedType_DontInitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        const string code = @"
class C
{ 
    class B
    {
    }
}";

        var immutableCompilation1 = testContext.CreateCompilationModel( code );
        Assert.Empty( immutableCompilation1.Types.Single().Methods );

        var mutableCompilation1 = immutableCompilation1.CreateMutableClone();

        var type1 = Assert.Single( mutableCompilation1.Types );

        // Add a type.
        var typeBuilder = new NamedTypeBuilder( null!, type1, "T" ) { BaseType = type1.Types.Single() };
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var baseType = Assert.Single( type2.Types.OfName( "B" ) );
        var introducedType = Assert.Single( type2.Types.OfName( "T" ) );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, baseType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( baseType.Methods.OfName( "M" ) );
        Assert.Single( introducedType.AllMethods.OfName( "M" ) );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().Types.OfName( "B" ).Single().Methods.OfName( "M" ) );
        Assert.Empty( immutableCompilation2.Types.Single().Types.OfName( "T" ).Single().AllMethods.OfName( "M" ) );
    }

    [Fact]
    public void ReplaceImplicitConstructor()
    {
        using var testContext = this.CreateTestContext();

        const string code =
            """
            class Target;
            """;

        var initialCompilation = testContext.CreateCompilationModel( code );

        var target = initialCompilation.Types.OfName( "Target" ).Single();

        var introducedConstructor = new ConstructorBuilder( null!, target );
        introducedConstructor.AddParameter( "p", typeof(int) );
        introducedConstructor.ReplacedImplicit = target.Constructors.Single().ToTypedRef();

        var implicitCtor = Assert.Single( target.Constructors );

        var finalCompilation = initialCompilation.WithTransformationsAndAspectInstances(
            [introducedConstructor.ToTransformation()],
            null,
            null );

        var target2 = finalCompilation.Types.OfName( "Target" ).Single();
        var constructor2 = Assert.Single( target2.Constructors );

        Assert.Equal( implicitCtor.Name, introducedConstructor.Name );
        Assert.Same( constructor2, introducedConstructor.ForCompilation<IConstructor>( finalCompilation ) );

        // NB: This a weird. It's caused by the introduced constructor replacing the implicit one.
        //     If another parameterless constructor is introduced afterwards, the implicit constructor will still translate to the
        //     one with parameters.

        Assert.Same( constructor2, implicitCtor.ForCompilation( finalCompilation ) );
    }

    [Fact]
    public void ReplaceImplicitStaticConstructor()
    {
        using var testContext = this.CreateTestContext();

        const string code =
            """
            class Target
            {
                public static int f = 42;
            }
            """;

        var initialCompilation = testContext.CreateCompilationModel( code );

        var target = initialCompilation.Types.OfName( "Target" ).Single();

        var introducedStaticConstructor =
            new ConstructorBuilder( null!, target ) { IsStatic = true, ReplacedImplicit = target.StaticConstructor.AssertNotNull().ToTypedRef() };

        Assert.NotNull( target.StaticConstructor );

        var finalCompilation = initialCompilation.WithTransformationsAndAspectInstances(
            [introducedStaticConstructor.ToTransformation()],
            null,
            null );

        var target2 = finalCompilation.Types.OfName( "Target" ).Single();
        var staticConstructor2 = target2.StaticConstructor;

        Assert.Equal( target.StaticConstructor.Name, introducedStaticConstructor.Name );
        Assert.Same( introducedStaticConstructor.ForCompilation<IConstructor>( finalCompilation ), staticConstructor2 );
        Assert.Same( target.StaticConstructor.ForCompilation( finalCompilation ), staticConstructor2 );
    }

    [Fact]
    public void DerivedTypes()
    {
        using var testContext = this.CreateTestContext();

        const string code =
            """
            interface I;

            class Target;
            """;

        var initialCompilation = testContext.CreateCompilationModel( code );

        var target = initialCompilation.Types.OfName( "Target" ).Single();

        var baseType = new NamedTypeBuilder( null!, target, "B" );

        var derivedType = new NamedTypeBuilder( null!, target, "C" ) { BaseType = baseType };

        var interfaceType = initialCompilation.Types.OfName( "I" ).Single();

        var implementInterface = new IntroduceInterfaceTransformation( null!, derivedType, interfaceType, [] );

        var finalCompilation = initialCompilation.WithTransformationsAndAspectInstances(
            [baseType.ToTransformation(), derivedType.ToTransformation(), implementInterface],
            null,
            null );

        var baseClass = finalCompilation.Types.OfName( "Target" ).Single().Types.OfName( "B" ).Single();

        var derivedClass = Assert.Single( finalCompilation.GetDerivedTypes( baseClass ) );

        Assert.Equal( "C", derivedClass.Name );

        var implementedInterface = finalCompilation.Types.OfName( "I" ).Single();

        var implementingClass = Assert.Single( finalCompilation.GetDerivedTypes( implementedInterface ) );

        Assert.Same( derivedClass, implementingClass );
    }

    [Fact]
    public void InterfaceImplementation()
    {
        using var testContext = this.CreateTestContext();

        const string code =
            """
            interface I
            {
                void Foo();
            }

            class Target;

            class Derived : Target;
            """;

        var initialCompilation = testContext.CreateCompilationModel( code );

        var targetType = initialCompilation.Types.OfName( "Target" ).Single();
        var interfaceType = initialCompilation.Types.OfName( "I" ).Single();
        var interfaceMethod = interfaceType.Methods.Single();

        var implementedInterfaceMethod = new MethodBuilder( null!, targetType, "Foo" );

        var implementInterface = new IntroduceInterfaceTransformation(
            null!,
            targetType,
            interfaceType,
            new() { [interfaceMethod] = implementedInterfaceMethod } );

        var finalCompilation = initialCompilation.WithTransformationsAndAspectInstances(
            [implementedInterfaceMethod.ToTransformation(), implementInterface],
            null,
            null );

        var targetType2 = finalCompilation.Types.OfName( "Target" ).Single();
        var derivedType2 = finalCompilation.Types.OfName( "Derived" ).Single();
        var interfaceType2 = finalCompilation.Types.OfName( "I" ).Single();
        var interfaceMethod2 = interfaceType2.Methods.Single();
        var targetTypeMethod2 = targetType2.Methods.Single();

        var implementations2 = finalCompilation.GetDerivedTypes( interfaceType2 ).ToArray();

        Assert.Single( targetType2.ImplementedInterfaces );
        Assert.Single( targetType2.AllImplementedInterfaces );
        Assert.Contains( targetType2, implementations2 );
        Assert.Contains( derivedType2, implementations2 );

        Assert.Empty( derivedType2.ImplementedInterfaces );
        Assert.Single( derivedType2.AllImplementedInterfaces );

        Assert.True( targetType2.TryFindImplementationForInterfaceMember( interfaceMethod2, out var implementedMethod ) );
        Assert.Same( targetTypeMethod2, implementedMethod );

        Assert.True( derivedType2.TryFindImplementationForInterfaceMember( interfaceMethod2, out var derivedImplementedMethod ) );
        Assert.Same( targetTypeMethod2, derivedImplementedMethod );
    }
}