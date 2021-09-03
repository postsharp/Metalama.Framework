// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CodeModel
{
    public class TypedConstantTests : TestBase
    {
        [Fact]
        public void Unassigned()
        {
            TypedConstant c = default;
            Assert.False( c.IsAssigned );
            Assert.Throws<ArgumentNullException>( () => c.Type );
            Assert.Throws<ArgumentNullException>( () => c.Value );
            Assert.Throws<ArgumentNullException>( () => c.IsNull );
        }
        
        [Fact]
        public void Assigned()
        {
            var emptyCompilation = CreateCompilationModel( "" );
            var c = new TypedConstant(emptyCompilation.Factory.GetSpecialType( SpecialType.Int32 ), 1) ;
            Assert.True( c.IsAssigned );
            Assert.NotNull(  c.Type );
            Assert.NotNull( c.Value );
            Assert.False( c.IsNull );
        }
        
        [Fact]
        public void Null()
        {
            var emptyCompilation = CreateCompilationModel( "" );
            var c = new TypedConstant(emptyCompilation.Factory.GetSpecialType( SpecialType.String ), null) ;
            Assert.True( c.IsAssigned );
            Assert.NotNull(  c.Type );
            Assert.Null( c.Value );
            Assert.True( c.IsNull );
        }
    }
}