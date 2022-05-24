// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.Builders;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel;

public class CodeModelUpdateTests : TestBase
{
    [Fact]
    public void AddMethodToEmptyType_InitializeBefore_Complete()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var compilation = testContext.CreateCompilationModel( code ).ToMutable();

        var type = Assert.Single( compilation.Types )!;

        // Instantiate the memoize the collection of methods.
        Assert.Empty( type.Methods );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M", ObjectReader.Empty );
        compilation.AddTransformation( methodBuilder );

        // Assert that the method has been added.
        Assert.Single( type.Methods );
    }

    [Fact]
    public void AddMethodToEmptyType_InitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var compilation = testContext.CreateCompilationModel( code ).ToMutable();

        var type = Assert.Single( compilation.Types )!;

        // Instantiate the memoize the collection of methods.
        Assert.Empty( type.Methods.OfName( "M" ) );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M", ObjectReader.Empty );
        compilation.AddTransformation( methodBuilder );

        // Assert that the method has been added.
        Assert.Single( type.Methods.OfName( "M" ) );
    }

    [Fact]
    public void AddMethodToEmptyType_DontInitializeBefore_Complete()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var compilation = testContext.CreateCompilationModel( code ).ToMutable();

        var type = Assert.Single( compilation.Types )!;

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M", ObjectReader.Empty );
        compilation.AddTransformation( methodBuilder );

        // Assert that the method has been added.
        Assert.Single( type.Methods );
    }

    [Fact]
    public void AddMethodToEmptyType_DontInitializeBefore_ByName()
    {
        using var testContext = this.CreateTestContext();

        var code = @"
class C
{    
}";

        var compilation = testContext.CreateCompilationModel( code ).ToMutable();

        var type = Assert.Single( compilation.Types )!;

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M", ObjectReader.Empty );
        compilation.AddTransformation( methodBuilder );

        // Assert that the method has been added.
        Assert.Single( type.Methods.OfName( "M" ) );
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

        var compilation = testContext.CreateCompilationModel( code ).ToMutable();

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

        var compilation = testContext.CreateCompilationModel( code ).ToMutable();

        var type = Assert.Single( compilation.Types )!;

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M", ObjectReader.Empty );
        compilation.AddTransformation( methodBuilder );

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

        var compilation = testContext.CreateCompilationModel( code ).ToMutable();

        var type = Assert.Single( compilation.Types )!;

        // Instantiate the memoize the collection of methods.
        Assert.Single( type.Methods.OfName( "M" ) );

        // Add a method.
        var methodBuilder = new MethodBuilder( null!, type, "M", ObjectReader.Empty );
        compilation.AddTransformation( methodBuilder );

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

        var compilation = testContext.CreateCompilationModel( code ).ToMutable();

        var type = Assert.Single( compilation.Types )!;

        // Instantiate the memoize the collection of fields.
        Assert.Empty( type.Fields );

        // Add a field.
        var fieldBuilder = new FieldBuilder( null!, type, "F", ObjectReader.Empty );
        compilation.AddTransformation( fieldBuilder );

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

        var compilation = testContext.CreateCompilationModel( code ).ToMutable();

        var type = Assert.Single( compilation.Types )!;

        // Add a field.
        var fieldBuilder = new FieldBuilder( null!, type, "F", ObjectReader.Empty );
        compilation.AddTransformation( fieldBuilder );

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

        var compilation = testContext.CreateCompilationModel( code ).ToMutable();

        var type = Assert.Single( compilation.Types )!;

        // Instantiate the memoize the collection of fields.
        Assert.Empty( type.Fields.OfName( "F" ) );

        // Add a field.
        var fieldBuilder = new FieldBuilder( null!, type, "F", ObjectReader.Empty );
        compilation.AddTransformation( fieldBuilder );

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

        var compilation = testContext.CreateCompilationModel( code ).ToMutable();

        var type = Assert.Single( compilation.Types )!;

        // Add a field.
        var fieldBuilder = new FieldBuilder( null!, type, "F", ObjectReader.Empty );
        compilation.AddTransformation( fieldBuilder );

        // Assert that the method has been added.
        Assert.Single( type.Fields.OfName( "F" ) );
    }
}