using System;
using Caravela.Framework.Impl.Serialization;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class TimeSpanSerializerTests
    {
        [Fact]
        public void TestTimeSpan()
        {
            var serializer = new TimeSpanSerializer();
            var ts = TimeSpan.FromMinutes( 38 );
            var ticks = 38 * TimeSpan.TicksPerMinute;
            Xunit.Assert.Equal( "new System.TimeSpan(" + ticks.ToString() + "L)", serializer.Serialize( ts ).ToString() );
        }

        [Fact]
        public void TestZero()
        {
            var serializer = new TimeSpanSerializer();
            Xunit.Assert.Equal( "new System.TimeSpan(0L)", serializer.Serialize( TimeSpan.Zero ).ToString() );
        }
    }
}