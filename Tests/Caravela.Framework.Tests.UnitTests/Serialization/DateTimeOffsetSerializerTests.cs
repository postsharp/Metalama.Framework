// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class DateTimeOffsetSerializerTests : SerializerTestsBase
    {
        [Fact]
        public void TestDateTimeOffset()
        {
            this.AssertDateTimeSerialization( new DateTimeOffset( 2000, 1, 1, 14, 42, 22, TimeSpan.Zero ) );
            this.AssertDateTimeSerialization( new DateTimeOffset( 2000, 1, 1, 14, 42, 22, TimeSpan.FromHours( 1 ) ) );
            this.AssertDateTimeSerialization( DateTimeOffset.MinValue );
            this.AssertDateTimeSerialization( DateTimeOffset.MaxValue );
        }

        private void AssertDateTimeSerialization( DateTimeOffset dateTime )
        {
            var dt = dateTime;
            Assert.Equal( dateTime, DateTimeOffset.Parse( dateTime.ToString( "o" ) ) );
            Assert.Equal( "global::System.DateTimeOffset.Parse(\"" + dateTime.ToString( "o" ) + "\")", this.Serialize( dt ).ToString() );
        }
    }
}