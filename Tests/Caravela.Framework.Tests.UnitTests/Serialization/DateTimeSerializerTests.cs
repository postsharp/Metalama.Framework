// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Serialization;
using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
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
            Assert.Equal( "System.DateTime.FromBinary(" + dt.ToBinary() + "L)", serializer.Serialize( dt ).ToString() );
        }
    }
}