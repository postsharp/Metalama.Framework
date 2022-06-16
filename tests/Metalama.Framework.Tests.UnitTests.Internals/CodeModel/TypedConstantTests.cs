// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
            var c = new TypedConstant( emptyCompilation.Factory.GetSpecialType( SpecialType.Int32 ), 1 );
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
            var c = new TypedConstant( emptyCompilation.Factory.GetSpecialType( SpecialType.String ), null );
            Assert.True( c.IsInitialized );
            Assert.NotNull( c.Type );
            Assert.Null( c.Value );
            Assert.True( c.IsNullOrDefault );
        }
    }
}