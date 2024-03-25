// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Testing.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using SymbolEqualityComparer = Microsoft.CodeAnalysis.SymbolEqualityComparer;

namespace Metalama.Framework.Tests.UnitTests.Utilities;

public sealed class SerializableTypeIdTests : UnitTestClass
{
    public SerializableTypeIdTests( ITestOutputHelper? testOutputHelper ) : base( testOutputHelper )
    {
        var compilation = TestCompilationFactory.CreateCSharpCompilation( "" );
        this._resolver = new SerializableTypeIdResolver( compilation );
        this._reflectionMapper = new ReflectionMapper( compilation );
    }

    private readonly SerializableTypeIdResolver _resolver;
    private readonly ReflectionMapper _reflectionMapper;

    [Theory]
    [InlineData( typeof(int) )]
    [InlineData( typeof(void) )]
    [InlineData( typeof(object) )]
    [InlineData( typeof(object[]) )]
    [InlineData( typeof(int*) )]
    [InlineData( typeof(int[]) )]
    [InlineData( typeof(decimal) )]
    [InlineData( typeof(List<decimal>) )]
    [InlineData( typeof(List<int[]>) )]
    [InlineData( typeof(List<>) )]
    [InlineData( typeof(Dictionary<,>) )]
    [InlineData( typeof(Dictionary<List<string>, List<int>>) )]
    public void TestTypeOf( Type type )
    {
        var symbol = this._reflectionMapper.GetTypeSymbol( type );
        var id = symbol.GetSerializableTypeId();
        this.TestOutput.WriteLine( id.Id );
        var roundTripType = this._resolver.ResolveId( id );
        Assert.True( SymbolEqualityComparer.Default.Equals( symbol, roundTripType ) );
    }

    [Theory]
    [InlineData( "object" )]
    [InlineData( "object?" )]
    [InlineData( "Task<object>" )]
    [InlineData( "Task<object?>" )]
    public void TestNullableType( string type )
    {
        using var testContext = this.CreateTestContext();

        var code = $"using System.Threading.Tasks;"
                   + $"class C {{ {type} f; }}";

        var compilation = testContext.CreateCompilation( code );
        var typeModel = compilation.Types.Single().Fields.Single().Type;
        var typeId = typeModel.ToSerializableId();
        var roundloop = compilation.GetCompilationModel().CompilationContext.SerializableTypeIdResolver.ResolveId( typeId );
        Assert.Equal( typeModel.GetSymbol(), roundloop, SymbolEqualityComparer.IncludeNullability );
    }
}