// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.ExpressionBuilders;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Templating
{
    public class InterpolatedStringBuilderTests
    {
        [Fact]
        public void TestClone()
        {
            var builder = new InterpolatedStringBuilder();

            // Note that production code always sends an expression (not its value) to the builder.
            builder.AddText( "1" );
            builder.AddExpression( 2 );

            var clone = builder.Clone();

            Assert.NotSame( clone, builder );
            Assert.Equal( builder.Items, clone.Items );

            builder.AddText( "4" );
            Assert.NotEqual( builder.Items, clone.Items );
        }
    }
}