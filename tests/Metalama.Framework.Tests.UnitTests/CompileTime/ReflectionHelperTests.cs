﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

// ReSharper disable UnusedTypeParameter, MemberCanBePrivate.Global, ClassNeverInstantiated.Global, MemberCanBeInternal, ClassCanBeSealed.Global

namespace Metalama.Framework.Tests.UnitTests.CompileTime;

public sealed class ReflectionHelperTests : UnitTestClass
{
    private static string RemoveAssemblyQualification( string typeName )
    {
        var result = Regex.Replace( typeName, @"\[(((?!\[\[).)+?), [^]]*?\]", "$1" );

        if ( typeName == result )
        {
            return result;
        }

        return RemoveAssemblyQualification( result );
    }

    public static IEnumerable<object[]> Types()
        =>
        [
            [typeof(int)],
            [typeof(List<>)],
            [typeof(List<int>)],
            [typeof(List<List<int>>)],
            [typeof(List<>.Enumerator)],
            [typeof(List<int>.Enumerator)],
            [typeof(List<List<int>>.Enumerator)],
            [typeof(Outer<int, object>.Inner<string>)],
            [typeof(int[])],
            [typeof(List<int>[])],
            [typeof(List<int>.Enumerator[])],
            [typeof(int[,])],
            [typeof(int*)]
        ];

    [Theory]
    [MemberData( nameof(Types) )]
    public void GetReflectionNameTest( Type type )
    {
        var compilation = TestCompilationFactory.CreateEmptyCSharpCompilation( null )
            .AddReferences( MetadataReference.CreateFromFile( typeof(ReflectionHelperTests).Assembly.Location ) );

        var reflectionMapper = new ReflectionMapper( compilation );

        var typeSymbol = reflectionMapper.GetTypeSymbol( type );

        Assert.Equal( RemoveAssemblyQualification( type.FullName! ), typeSymbol.GetReflectionFullName() );
        Assert.Equal( type.Name, typeSymbol.GetReflectionName() );
        Assert.Equal( type.ToString(), typeSymbol.GetReflectionToStringName() );
    }

    [Theory]
    [MemberData( nameof(Types) )]
    public void GetDeclarationReflectionNameTest( Type type )
    {
        using var testContext = this.CreateTestContext();

        var compilation = TestCompilationFactory.CreateEmptyCSharpCompilation( null )
            .AddReferences( MetadataReference.CreateFromFile( typeof(ReflectionHelperTests).Assembly.Location ) );

        var reflectionMapper = new ReflectionMapper( compilation );

        var compilationModel = testContext.CreateCompilationModel( compilation );

        var typeSymbol = reflectionMapper.GetTypeSymbol( type );
        var iType = compilationModel.Factory.GetIType( typeSymbol );

        Assert.Equal( RemoveAssemblyQualification( type.FullName! ), iType.GetReflectionFullName( bypassSymbols: true ) );
        Assert.Equal( type.Name, iType.GetReflectionName( bypassSymbols: true ) );
    }

    public class Outer<T1, T2>
    {
        public class Inner<T3>;
    }
}