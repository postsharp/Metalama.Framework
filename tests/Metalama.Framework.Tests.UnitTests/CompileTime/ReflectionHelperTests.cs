// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

// ReSharper disable UnusedTypeParameter

namespace Metalama.Framework.Tests.UnitTests.CompileTime;

public sealed class ReflectionHelperTests
{
    [Theory]
    [InlineData( typeof(int) )]
    [InlineData( typeof(List<>) )]
    [InlineData( typeof(List<int>) )]
    [InlineData( typeof(List<List<int>>) )]
    [InlineData( typeof(List<>.Enumerator) )]
    [InlineData( typeof(List<int>.Enumerator) )]
    [InlineData( typeof(List<List<int>>.Enumerator) )]
    [InlineData( typeof(Outer<int, object>.Inner<string>) )]
    [InlineData( typeof(int[]) )]
    [InlineData( typeof(List<int>[]) )]
    [InlineData( typeof(List<int>.Enumerator[]) )]
    [InlineData( typeof(int[,]) )]
    [InlineData( typeof(int*) )]
    public void GetReflectionNameTest( Type type )
    {
        static string RemoveAssemblyQualification( string typeName )
        {
            var result = Regex.Replace( typeName, @"\[(((?!\[\[).)+?), [^]]*?\]", "$1" );

            if ( typeName == result )
            {
                return result;
            }

            return RemoveAssemblyQualification( result );
        }

        var compilation = TestCompilationFactory.CreateEmptyCSharpCompilation( null )
            .AddReferences( MetadataReference.CreateFromFile( typeof(ReflectionHelperTests).Assembly.Location ) );

        var reflectionMapper = new ReflectionMapper( compilation );

        var typeSymbol = reflectionMapper.GetTypeSymbol( type );

        Assert.Equal( RemoveAssemblyQualification( type.FullName! ), typeSymbol.GetReflectionFullName() );
        Assert.Equal( type.Name, typeSymbol.GetReflectionName() );
        Assert.Equal( type.ToString(), typeSymbol.GetReflectionToStringName() );
    }

    public class Outer<T1, T2>
    {
        public class Inner<T3> { }
    }
}