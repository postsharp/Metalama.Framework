// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public class TypedConstantTests : TestBase
    {
        [Fact]
        public void Unassigned()
        {
            TypedConstant c = default;
            Assert.False( c.IsInitialized );
            Assert.Throws<ArgumentNullException>( () => c.Type );
            Assert.Throws<ArgumentNullException>( () => c.Value );
            Assert.Throws<ArgumentNullException>( () => c.IsNullOrDefault );
        }

        [Fact]
        public void Assigned()
        {
            using var testContext = this.CreateTestContext();

            var emptyCompilation = testContext.CreateCompilationModel( "" );
            var c = TypedConstant.Create( 1, emptyCompilation.Factory.GetSpecialType( SpecialType.Int32 ) );
            Assert.True( c.IsInitialized );
            Assert.NotNull( c.Type );
            Assert.NotNull( c.Value );
            Assert.False( c.IsNullOrDefault );
        }

        [Fact]
        public void Null()
        {
            using var testContext = this.CreateTestContext();

            var emptyCompilation = testContext.CreateCompilationModel( "" );
            var c = TypedConstant.Create( null, emptyCompilation.Factory.GetSpecialType( SpecialType.String ) );
            Assert.True( c.IsInitialized );
            Assert.NotNull( c.Type );
            Assert.Null( c.Value );
            Assert.True( c.IsNullOrDefault );
        }
    }
}