﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AdviceImpl.InterfaceImplementation;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel;

public sealed partial class CodeModelUpdateTests
{
    [Fact]
    public void AddTypeToCompilation_VisibleInCompilationTypesCollection()
    {
        using var testContext = this.CreateTestContext();

        var compilation = testContext.CreateCompilationModel( "class Outer;" ).CreateMutableClone();

        var type = new NamedTypeBuilder( null!, compilation.GlobalNamespace, "C" );
        type.Freeze();
        compilation.AddTransformation( type.ToTransformation() );

        Assert.Single( compilation.GlobalNamespace.Types.OfName( "C" ) );
        Assert.Single( compilation.Types.OfName( "C" ) );

        var nestedType = new NamedTypeBuilder( null!, compilation.Types.OfName( "Outer" ).Single(), "Inner" );
        compilation.AddTransformation( nestedType.ToTransformation() );

        Assert.Single( compilation.GlobalNamespace.Types.OfName( "Outer" ).Single().Types );
        Assert.Single( compilation.AllTypes.OfName( "Inner" ) );
    }

    [Fact]
    public void AddTypeToIntroducedNamespace_IsVisibleInThatNamespace()
    {
        using var testContext = this.CreateTestContext();

        var compilation = testContext.CreateCompilationModel( "" ).CreateMutableClone();

        var nsBuilder = new NamespaceBuilder( null!, compilation.GlobalNamespace, "NS" );
        compilation.AddTransformation( nsBuilder.ToTransformation() );

        var typeBuilder = new NamedTypeBuilder( null!, nsBuilder, "C" );
        compilation.AddTransformation( typeBuilder.ToTransformation() );

        var ns = compilation.GlobalNamespace.GetDescendant( "NS" );

        Assert.NotNull( ns );

        Assert.Single( ns.Types.OfName( "C" ) );          
        Assert.Single( compilation.Types.OfName( "C" ) ); 
    }

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
        typeBuilder.Freeze();
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var nestedType = Assert.Single( type2.Types );

        // Instantiate the memoize the collection of methods of the nested type.
        Assert.Empty( nestedType.Methods );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        methodBuilder.Freeze();
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
        typeBuilder.Freeze();
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var nestedType = Assert.Single( type2.Types );

        // Instantiate the memoize the collection of methods of the nested type.
        Assert.Empty( nestedType.Methods.OfName( "M" ) );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        methodBuilder.Freeze();
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
        typeBuilder.Freeze();
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var nestedType = Assert.Single( type2.Types );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        methodBuilder.Freeze();
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
        typeBuilder.Freeze();
        mutableCompilation1.AddTransformation( typeBuilder.ToTransformation() );

        var immutableCompilation2 = mutableCompilation1.CreateImmutableClone();
        var mutableCompilation2 = immutableCompilation2.CreateMutableClone();

        var type2 = Assert.Single( mutableCompilation2.Types );
        var nestedType = Assert.Single( type2.Types );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        methodBuilder.Freeze();
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( nestedType.Methods.OfName( "M" ) );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().Types.Single().Methods.OfName( "M" ) );
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
        baseType.Freeze();

        var derivedType = new NamedTypeBuilder( null!, target, "C" ) { BaseType = baseType };
        derivedType.Freeze();

        var interfaceType = initialCompilation.Types.OfName( "I" ).Single();

        var implementInterface = new IntroduceInterfaceTransformation( null!, derivedType.ToRef(), interfaceType.ToRef(), [] );

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
}