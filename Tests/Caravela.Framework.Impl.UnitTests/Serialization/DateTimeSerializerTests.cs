using System;
using Caravela.Framework.Impl.Serialization;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class DateTimeSerializerTests
    {
        [Fact]
        public void TestDateTime()
        {
            AssertDateTimeSerialization( new DateTime( 2000, 1, 1, 14, 42, 22, DateTimeKind.Local ) );
            AssertDateTimeSerialization( new DateTime( 2000, 1, 1, 14, 42, 22, DateTimeKind.Utc ) );
            AssertDateTimeSerialization( DateTime.Now );
            AssertDateTimeSerialization( DateTime.MinValue );
            AssertDateTimeSerialization( DateTime.MaxValue );
        }

        private static void AssertDateTimeSerialization( DateTime dateTime )
        {
            var serializer = new DateTimeSerializer();
            var dt = dateTime;
            Xunit.Assert.Equal( "System.DateTime.FromBinary(" + dt.ToBinary().ToString() + "L)", serializer.Serialize( dt ).ToString() );
        }
    }
}