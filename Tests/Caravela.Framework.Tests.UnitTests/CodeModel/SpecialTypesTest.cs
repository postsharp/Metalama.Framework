// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CodeModel
{
    public class SpecialTypesTest : TestBase
    {
        [Theory]
        [InlineData( SpecialType.Byte )]
        [InlineData( SpecialType.Decimal )]
        [InlineData( SpecialType.Double )]
        [InlineData( SpecialType.Int16 )]
        [InlineData( SpecialType.Int32 )]
        [InlineData( SpecialType.Int64 )]
        [InlineData( SpecialType.Object )]
        [InlineData( SpecialType.Single )]
        [InlineData( SpecialType.String )]
        [InlineData( SpecialType.Task )]
        [InlineData( SpecialType.Task_T )]
        [InlineData( SpecialType.ValueTask )]
        [InlineData( SpecialType.ValueTask_T )]
        [InlineData( SpecialType.IEnumerable )]
        [InlineData( SpecialType.IEnumerator )]
        [InlineData( SpecialType.IEnumerable_T )]
        [InlineData( SpecialType.IEnumerator_T )]
        [InlineData( SpecialType.IAsyncEnumerable_T )]
        [InlineData( SpecialType.IAsyncEnumerator_T )]
        public void TestType( SpecialType type )
        {
            var emptyModel = this.CreateCompilationModel( "" );
            var namedType = emptyModel.Factory.GetSpecialType( type );
            Assert.Equal( type, namedType.SpecialType );
            Assert.True( namedType.Is( type ) );
        }
    }
}