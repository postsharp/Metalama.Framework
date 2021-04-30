// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class TimeSpanSerializerTests : SerializerTestsBase
    {
        [Fact]
        public void TestTimeSpan()
        {
            var ts = TimeSpan.FromMinutes( 38 );
            var ticks = 38 * TimeSpan.TicksPerMinute;
            Assert.Equal( "new global::System.TimeSpan(" + ticks + "L)", this.Serialize( ts ).ToString() );
        }

        [Fact]
        public void TestZero()
        {
            Assert.Equal( "new global::System.TimeSpan(0L)", this.Serialize( TimeSpan.Zero ).ToString() );
        }
    }
}