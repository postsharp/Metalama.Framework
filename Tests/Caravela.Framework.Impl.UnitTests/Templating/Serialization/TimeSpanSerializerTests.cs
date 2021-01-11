using Caravela.Framework.Impl.Templating.Serialization;
using System;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class TimeSpanSerializerTests
    {
        [Fact]
        public void TestTimeSpan()
        {
            var serializer = new TimeSpanSerializer();
            TimeSpan ts = TimeSpan.FromMinutes( 38 );
            long ticks = 38 * TimeSpan.TicksPerMinute;
            Assert.Equal("new System.TimeSpan(" + ticks.ToString() + "L)", serializer.Serialize(ts).ToString());
        }

        [Fact]
        public void TestZero()
        {         
            var serializer = new TimeSpanSerializer();
            Assert.Equal( "new System.TimeSpan(0L)", serializer.Serialize( TimeSpan.Zero ).ToString() );
        }
    }
}