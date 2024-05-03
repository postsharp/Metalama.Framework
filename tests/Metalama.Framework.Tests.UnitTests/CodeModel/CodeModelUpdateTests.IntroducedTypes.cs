﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Testing.UnitTesting;
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
        var nestedType = Assert.Single( type2.NestedTypes );

        // Instantiate the memoize the collection of methods of the nested type.
        Assert.Empty( nestedType.Methods );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( nestedType.Methods );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().NestedTypes.Single().Methods );
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
        var nestedType = Assert.Single( type2.NestedTypes );

        // Instantiate the memoize the collection of methods of the nested type.
        Assert.Empty( nestedType.Methods.OfName( "M" ) );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( nestedType.Methods.OfName( "M" ) );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().NestedTypes.Single().Methods.OfName( "M" ) );
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
        var nestedType = Assert.Single( type2.NestedTypes );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( nestedType.Methods );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().NestedTypes.Single().Methods );
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
        var nestedType = Assert.Single( type2.NestedTypes );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, nestedType, "M" );
        mutableCompilation2.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( nestedType.Methods.OfName( "M" ) );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation2.Types.Single().NestedTypes.Single().Methods.OfName( "M" ) );
    }
}