// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.ExpressionBuilders;
using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Templating
{
    public class ArrayBuilderTests : TestBase
    {
        [Fact]
        public void Clone()
        {
            var model = CreateCompilationModel( "" );
            var builder = new ArrayBuilder( model.Factory.GetSpecialType( SpecialType.Object ) );

            // Note that production code always sends an expression (not its value) to the builder.
            builder.Add( "1" );
            builder.Add( 2 );
            builder.Add( DateTime.Now );

            var clone = builder.Clone();

            Assert.NotSame( clone, builder );
            Assert.Equal( builder.Items, clone.Items );

            builder.Add( 4 );
            Assert.NotEqual( builder.Items, clone.Items );
        }

        [Fact]
        public void OutOfContext()
        {
            Assert.Throws<InvalidOperationException>( () => new ArrayBuilder( typeof(int) ) );
            Assert.Throws<InvalidOperationException>( () => new ArrayBuilder() );
        }
    }
}