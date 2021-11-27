// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.
/*
using Caravela.Framework.Code;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Utilities.Dump;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CodeModel
{
    public class PublicApiDumperTests : TestBase
    {
        private static object? DumpClass<T>( T? obj )
            where T : class
        {
            var dump = ObjectDumper.Dump( obj );

            if ( obj != null )
            {
                Assert.NotNull( dump );
            }
            else
            {
                Assert.Null( dump );
            }

            return dump;
        }

        private static T ResolveLazy<T>( T inferTypeFrom, object? lazy )
        {
            Assert.IsType<Lazy<T>>( lazy );

            return ((Lazy<T>) lazy!).Value;
        }

        private static object? DumpStruct<T>( T obj )
            where T : struct
            => ObjectDumper.Dump( obj );

        [Fact]
        public void Tests()
        {
            using var testContext = this.CreateTestContext();
            var compilation = testContext.CreateCompilationModel( "class C {}" );
            var projectDump = (IDictionary<string, object?>) DumpClass( compilation.Project ).AssertNotNull();

            DumpStruct( compilation.Project.AssemblyReferences );
            DumpClass( compilation.Project.AssemblyReferences[0] );
            var publicKeyDump = DumpStruct( compilation.Project.AssemblyReferences[0].PublicKey );
            Assert.IsType<string>( publicKeyDump );

            var namedType = compilation.Types[0];
            var dumper = ObjectDumper.GetDumper( namedType.GetType() );
            Assert.True( dumper.Properties.Single( p => p.PropertyName == nameof(INamedType.Methods) ).IsLazy );
        }

        [Theory]
        [InlineData( typeof(int), false )]
        [InlineData( typeof(string), false )]
        [InlineData( typeof(ArrayList), true )]
        [InlineData( typeof(IEnumerable), true )]
        [InlineData( typeof(IEnumerable<int>), true )]
        [InlineData( typeof(ImmutableArray<int>), true )]
        public void IsLazy( Type type, bool isLazy )
        {
            Assert.Equal( isLazy, ObjectDumper.RequiresLazyLoad( type ) );
        }
    }
}*/