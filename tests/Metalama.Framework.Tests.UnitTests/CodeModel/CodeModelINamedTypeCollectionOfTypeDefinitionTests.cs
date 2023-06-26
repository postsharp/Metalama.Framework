// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Testing.UnitTesting;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public class CodeModelINamedTypeCollectionOfTypeDefinitionTests : UnitTestClass
    {
        [Fact]
        public void NonGeneric()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C
{
}
class D
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types.Single( t => t.Name == "C" );

            var types = compilation.Types.OfTypeDefinition( type );

            Assert.Equal( new[] { type }, types );
        }

        [Fact]
        public void Generic()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C<T>
{
}

class D
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types.Single( t => t.Name == "C" );

            var types = compilation.Types.OfTypeDefinition( type );

            Assert.Equal( new[] { type }, types );
        }
    }
}