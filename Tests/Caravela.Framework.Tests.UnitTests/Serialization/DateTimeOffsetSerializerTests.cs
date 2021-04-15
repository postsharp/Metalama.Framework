// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Serialization;
using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class DateTimeOffsetSerializerTests
    {
        [Fact]
        public void TestDateTimeOffset()
        {
            AssertDateTimeSerialization( new DateTimeOffset( 2000, 1, 1, 14, 42, 22, TimeSpan.Zero ) );
            AssertDateTimeSerialization( new DateTimeOffset( 2000, 1, 1, 14, 42, 22, TimeSpan.FromHours( 1 ) ) );
            AssertDateTimeSerialization( DateTimeOffset.MinValue );
            AssertDateTimeSerialization( DateTimeOffset.MaxValue );
        }

        private static void AssertDateTimeSerialization( DateTimeOffset dateTime )
        {
            var serializer = new DateTimeOffsetSerializer();
            var dt = dateTime;
            Assert.Equal( dateTime, DateTimeOffset.Parse( dateTime.ToString( "o" ) ) );
            Assert.Equal( "System.DateTimeOffset.Parse(\"" + dateTime.ToString( "o" ) + "\")", serializer.Serialize( dt ).ToString() );
        }
    }
}