// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Impl.Serialization;
using Xunit;

namespace Caravela.Framework.UnitTests.Templating.Serialization
{
    public class TimeSpanSerializerTests
    {
        [Fact]
        public void TestTimeSpan()
        {
            var serializer = new TimeSpanSerializer();
            var ts = TimeSpan.FromMinutes( 38 );
            var ticks = 38 * TimeSpan.TicksPerMinute;
            Assert.Equal( "new System.TimeSpan(" + ticks.ToString() + "L)", serializer.Serialize( ts ).ToString() );
        }

        [Fact]
        public void TestZero()
        {
            var serializer = new TimeSpanSerializer();
            Assert.Equal( "new System.TimeSpan(0L)", serializer.Serialize( TimeSpan.Zero ).ToString() );
        }
    }
}