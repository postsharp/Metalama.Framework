// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.TestFramework.Utilities;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public class CompileTimeTypeTests : TestBase
    {
        [Theory]
        [InlineData( typeof(Task) )]
        [InlineData( typeof(Task<>) )]
        [InlineData( typeof(Task<int>) )]
        [InlineData( typeof(Task[]) )]
        [InlineData( typeof(Task<int>[]) )]
        [InlineData( typeof(Task<int[]>) )]
        public void Test( Type type )
        {
            using var testContext = this.CreateTestContext();
            var compilation = testContext.CreateCompilationModel( "/* Intentionally empty */" );

            var reflectionMapper = new ReflectionMapper( compilation.RoslynCompilation );
            var typeSymbol = reflectionMapper.GetTypeSymbol( type );
            var compileTimeType = (CompileTimeType) new CompileTimeTypeFactory().Get( typeSymbol );

            var expectedTypeName = type.FullName?
#if NET5_0_OR_GREATER
                .ReplaceOrdinal( ", System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", "" )
#else
                .ReplaceOrdinal( ", mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "" )
#endif
                .ReplaceOrdinal( "[[", "[" )
                .ReplaceOrdinal( "]]", "]" );

            Assert.Equal( expectedTypeName, compileTimeType.FullName );

            var resolvedType = compileTimeType.Target.GetTarget( compilation );

            Assert.NotNull( resolvedType );
        }
    }
}