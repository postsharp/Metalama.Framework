// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Testing;
using Caravela.Framework.LinqPad;
using Xunit;

namespace Caravela.Framework.Tests.Workspaces
{
    public class FacadeObjectTests : TestBase
    {
        private static object? DumpClass<T>( T? obj )
            where T : class
        {
            var dump = ObjectFacadeFactory.GetFacade( obj );

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
            => ObjectFacadeFactory.GetFacade( obj );

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

            var namedType = compilation.Types[0];
            var type = ObjectFacadeFactory.GetFacade( namedType )!.Type;
            Assert.Contains( "Methods", type.PropertyNames );
            Assert.Contains( "DeclaringAssembly", type.PropertyNames );
        }
    }
}