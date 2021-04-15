// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Serialization;
using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class TimeSpanSerializerTests
    {
        [Fact]
        public void TestTimeSpan()
        {
            var serializer = new TimeSpanSerializer();
            var ts = TimeSpan.FromMinutes( 38 );
            var ticks = 38 * TimeSpan.TicksPerMinute;
            Assert.Equal( "new System.TimeSpan(" + ticks + "L)", serializer.Serialize( ts ).ToString() );
        }

        [Fact]
        public void TestZero()
        {
            var serializer = new TimeSpanSerializer();
            Assert.Equal( "new System.TimeSpan(0L)", serializer.Serialize( TimeSpan.Zero ).ToString() );
        }
    }
}