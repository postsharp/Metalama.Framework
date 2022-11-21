﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Utilities;

public class SerializableTypeIdTests : LoggingTestBase
{
    public SerializableTypeIdTests( ITestOutputHelper? testOutputHelper ) : base( testOutputHelper )
    {
        var compilation = CreateCSharpCompilation( "" );
        this._provider = new SerializableTypeIdProvider( compilation );
        this._reflectionMapper = new ReflectionMapper( compilation );
    }

    private readonly SerializableTypeIdProvider _provider;
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
    public void TestType( Type type )
    {
        var symbol = this._reflectionMapper.GetTypeSymbol( type );
        var id = SerializableTypeIdProvider.GetId( symbol );
        this.Logger.WriteLine( id.Id );
        var roundTripType = this._provider.ResolveId( id );
        Assert.True( SymbolEqualityComparer.Default.Equals( symbol, roundTripType ) );
    }
}