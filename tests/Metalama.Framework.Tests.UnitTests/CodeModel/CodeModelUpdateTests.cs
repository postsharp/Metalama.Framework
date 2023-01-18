﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using Metalama.Testing.UnitTesting;
using System;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel;

public sealed class CodeModelUpdateTests : UnitTestClass
{
    [Fact]
    public void AddMethodToEmptyType_InitializeBefore_Complete()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        Assert.Empty( immutableCompilation.Types.Single().Methods );

        var compilation = immutableCompilation.CreateMutableClone();

        var type = Assert.Single( compilation.Types )!;

        // Instantiate the memoize the collection of methods.
        Assert.Empty( type.Methods );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M" );
        compilation.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( type.Methods );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation.Types.Single().Methods );
    }

    [Fact]
    public void AddMethodToEmptyType_InitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        Assert.Empty( immutableCompilation.Types.Single().Methods.OfName( "M" ) );

        var compilation = immutableCompilation.CreateMutableClone();

        var type = Assert.Single( compilation.Types )!;

        // Instantiate the memoize the collection of methods.
        Assert.Empty( type.Methods.OfName( "M" ) );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M" );
        compilation.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( type.Methods.OfName( "M" ) );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation.Types.Single().Methods.OfName( "M" ) );
    }

    [Fact]
    public void AddMethodToEmptyType_DontInitializeBefore_Complete()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        var type = Assert.Single( compilation.Types )!;

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M" );
        compilation.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( type.Methods );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation.Types.Single().Methods );
    }

    [Fact]
    public void AddMethodToEmptyType_DontInitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        var type = Assert.Single( compilation.Types )!;

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M" );
        compilation.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( type.Methods.OfName( "M" ) );

        // Assert that there is still no method in original compilation.
        Assert.Empty( immutableCompilation.Types.Single().Methods.OfName( "M" ) );
    }

    [Fact]
    public void NonEmptyType_GetByName_Then_GetAll()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
  void M() {}
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        var type = Assert.Single( compilation.Types )!;

        // Assert that the method has been added.
        Assert.Single( type.Methods.OfName( "M" ) );
        Assert.Single( type.Methods );
    }

    [Fact]
    public void AddMethodToNonEmptyType_DontInitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
void M(int p){}
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        var type = Assert.Single( compilation.Types )!;

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M" );
        compilation.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Equal( 2, type.Methods.OfName( "M" ).Count() );
    }

    [Fact]
    public void AddMethodToNonEmptyType_InitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
void M(int p){}
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        var type = Assert.Single( compilation.Types )!;

        // Instantiate the memoize the collection of methods.
        Assert.Single( type.Methods.OfName( "M" ) );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M" );
        compilation.AddTransformation( methodBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Equal( 2, type.Methods.OfName( "M" ).Count() );
    }

    [Fact]
    public void AddFieldToEmptyType_InitializeBefore_Complete()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        var type = Assert.Single( compilation.Types )!;

        // Instantiate the memoize the collection of fields.
        Assert.Empty( type.Fields );

        // Add a field.
        var fieldBuilder = new FieldBuilder( null!, type, "F", ObjectReader.Empty );
        compilation.AddTransformation( fieldBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( type.Fields );
    }

    [Fact]
    public void AddFieldToEmptyType_DontInitializeBefore_Complete()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        var type = Assert.Single( compilation.Types )!;

        // Add a field.
        var fieldBuilder = new FieldBuilder( null!, type, "F", ObjectReader.Empty );
        compilation.AddTransformation( fieldBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( type.Fields );
    }

    [Fact]
    public void AddFieldToEmptyType_InitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        var type = Assert.Single( compilation.Types )!;

        // Instantiate the memoize the collection of fields.
        Assert.Empty( type.Fields.OfName( "F" ) );

        // Add a field.
        var fieldBuilder = new FieldBuilder( null!, type, "F", ObjectReader.Empty );
        compilation.AddTransformation( fieldBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( type.Fields.OfName( "F" ) );
    }

    [Fact]
    public void AddFieldToEmptyType_DontInitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        var type = Assert.Single( compilation.Types )!;

        // Add a field.
        var fieldBuilder = new FieldBuilder( null!, type, "F", ObjectReader.Empty );
        compilation.AddTransformation( fieldBuilder.ToTransformation() );

        // Assert that the method has been added.
        Assert.Single( type.Fields.OfName( "F" ) );
    }

    [Fact]
    public void AddParameterToExplicitConstructor()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
   public C() {}
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        var constructor = compilation.Types.Single().Constructors.Single();

        // Add a field.
        var parameterBuilder = new ParameterBuilder( constructor, 0, "p", compilation.Factory.GetTypeByReflectionType( typeof(int) ), RefKind.In, null! );
        compilation.AddTransformation( new IntroduceParameterTransformation( null!, parameterBuilder ) );

        Assert.Single( constructor.Parameters );
    }

    [Fact]
    public void AddParameterToImplicitConstructor()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
  
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        var constructor = compilation.Types.Single().Constructors.Single();

        // Add a field.
        var parameterBuilder = new ParameterBuilder( constructor, 0, "p", compilation.Factory.GetTypeByReflectionType( typeof(int) ), RefKind.In, null! );
        compilation.AddTransformation( new IntroduceParameterTransformation( null!, parameterBuilder ) );

        Assert.Single( constructor.Parameters );
    }

    [Fact]
    public void AddAttribute_Type()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
  
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();
        var type = compilation.Types.Single();

        Assert.Empty( type.Attributes );

        compilation.AddTransformation(
            new AttributeBuilder(
                    null!,
                    type,
                    AttributeConstruction.Create( (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(SerializableAttribute) ) ) )
                .ToTransformation() );

        Assert.Single( type.Attributes );
    }

    [Fact]
    public void AddAttribute_Compilation()
    {
        using var testContext = this.CreateTestContext();

        var code = @"";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();

        Assert.Empty( compilation.Attributes );

        compilation.AddTransformation(
            new AttributeBuilder(
                    null!,
                    compilation,
                    AttributeConstruction.Create( (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(SerializableAttribute) ) ) )
                .ToTransformation() );

        Assert.Single( compilation.Attributes );
    }

    [Fact]
    public void RemoveAttribute_Type()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
[System.Serializable]
class C
{    
  
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var compilation = immutableCompilation.CreateMutableClone();
        var type = compilation.Types.Single();

        Assert.Single( type.Attributes );

        compilation.AddTransformation(
            new RemoveAttributesTransformation(
                null!,
                type,
                (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(SerializableAttribute) ) ) );

        Assert.Empty( type.Attributes );
    }
}