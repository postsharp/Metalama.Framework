// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class DateTimeSerializerTests : SerializerTestsBase
    {
        [Fact]
        public void TestDateTime()
        {
            this.AssertDateTimeSerialization( new DateTime( 2000, 1, 1, 14, 42, 22, DateTimeKind.Local ) );
            this.AssertDateTimeSerialization( new DateTime( 2000, 1, 1, 14, 42, 22, DateTimeKind.Utc ) );
            this.AssertDateTimeSerialization( DateTime.Now );
            this.AssertDateTimeSerialization( DateTime.MinValue );
            this.AssertDateTimeSerialization( DateTime.MaxValue );
        }

        private void AssertDateTimeSerialization( DateTime dateTime )
        {
            using var testContext = this.CreateTestContext();

            var dt = dateTime;
            Assert.Equal( "global::System.DateTime.FromBinary(" + dt.ToBinary() + "L)", testContext.Serialize( dt ).ToString() );
        }
    }
}