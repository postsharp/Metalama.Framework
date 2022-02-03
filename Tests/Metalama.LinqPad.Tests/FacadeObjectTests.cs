// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Testing;
using Metalama.LinqPad.Tests.Assets;
using System;
using System.Linq;
using Xunit;

namespace Metalama.LinqPad.Tests
{
    public class FacadeObjectTests : TestBase
    {
        private static readonly FacadeObjectFactory _facadeObjectFactory = new();

        private static object? DumpClass<T>( T? obj )
            where T : class
        {
            var dump = _facadeObjectFactory.GetFacade( obj );

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

        private static object? DumpStruct<T>( T obj )
            where T : struct
            => _facadeObjectFactory.GetFacade( obj );

        [Fact]
        public void Tests()
        {
            using var testContext = this.CreateTestContext();
            var compilation = testContext.CreateCompilation( "class C {}" );

            Assert.NotNull( DumpClass( compilation.Project ) );
            Assert.Null( DumpStruct( compilation.Project.AssemblyReferences ) );
            Assert.NotNull( DumpClass( compilation.Project.AssemblyReferences[0] ) );
            Assert.Null( DumpStruct( compilation.Project.AssemblyReferences[0].PublicKey ) );
        }

        [Fact]
        public void InheritedInterfacePropertiesAreAvailable()
        {
            using var testContext = this.CreateTestContext();
            var compilation = testContext.CreateCompilation( "class C {}" );

            var type = _facadeObjectFactory.GetFormatterType( compilation.Types[0].GetType() );
            Assert.Contains( "Methods", type.PropertyNames );
            Assert.Contains( "DeclaringAssembly", type.PropertyNames );
        }

        [Fact]
        public void ValueTupleTest()
        {
            var type = _facadeObjectFactory.GetFormatterType( typeof(ValueTuple<int, string>) );
            Assert.Contains( "Item1", type.PropertyNames );
            Assert.Contains( "Item2", type.PropertyNames );
        }

        [Fact]
        public void AnonymousTypeTest()
        {
            var o = new { Id = 1, Name = "name" };
            var type = _facadeObjectFactory.GetFormatterType( o.GetType() );
            Assert.Contains( "Id", type.PropertyNames );
            Assert.Contains( "Name", type.PropertyNames );
        }

        [Fact]
        public void PropertyHiding()
        {
            var type = _facadeObjectFactory.GetFormatterType( typeof(Impl) );
            Assert.Single( type.PropertyTypes );
            var propertyType = type.PropertyTypes.First();
            Assert.Equal( typeof(string), propertyType );
        }
    }
}