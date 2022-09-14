// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.SyntaxSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
// ReSharper disable MemberCanBePrivate.Global

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization.Reflection;

public class ReflectionSignatureBuilderTests : TestBase
{
    [Fact]
    public void TestGetMethodSignature()
    {
        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( "" );
        var modelType = (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(C<>) );

        foreach ( var reflectionMethod in typeof(C<>).GetMethods( BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) )
        {
            var modelMethod = modelType.Methods.OfName( reflectionMethod.Name ).Single();

            var signature = ReflectionSignatureBuilder.GetMethodSignature( modelMethod );

            Assert.Equal( reflectionMethod.ToString(), signature );
        }
    }
    
    [Fact]
    public void TestHasTypeArgument()
    {
        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( "" );
        var modelType = (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(C<>) );

        foreach ( var reflectionMethod in typeof(C<>).GetMethods( BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly ) )
        {
            var modelMethod = modelType.Methods.OfName( reflectionMethod.Name ).Single();

            var hasTypeArgument = ReflectionSignatureBuilder.HasTypeArgument( modelMethod );

            Assert.Equal( reflectionMethod.ToString().Contains( "TypeArgument" ), hasTypeArgument );
        }
    }

    // The class and the methods must be public otherwise they are not present in the reference assembly that visible from the compilation model.
    public class C<TypeArgument1>
    {
        public void M1() { }

        public void M2( TypeArgument1 x, dynamic y ) { }

        public void M3( int p1, object p2, bool p3, byte p4, sbyte p5, Enum e, decimal d ) { }

        public void M4<TypeArgument2>( TypeArgument2 x ) { }

        public void M5<TypeArgument2, TypeArgument3>( TypeArgument2 x, TypeArgument3 y ) { }

        public void M6<TypeArgument2>( out TypeArgument2? x )
        {
            x = default;
        }

        public unsafe void M7<TypeArgument2>( int* x ) { }

        public void M8<TypeArgument2>( TypeArgument2[] x, int[][] y, int[,] z ) { }

        public void M9<TypeArgument2>( List<TypeArgument2> x, Dictionary<int, string> y ) { }

        public void M10<TypeArgument2>( TypeArgument2? x ) { }

        public void M11<TypeArgument2>( TypeArgument2? x ) where TypeArgument2 : struct { }
    }
}