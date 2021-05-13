// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Globalization;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class CultureInfoSerializerTests : SerializerTestsBase
    {
        [Fact]
        public void TestCzech()
        {
            var ci = new CultureInfo( "cs-CZ", true );
            Assert.Equal( @"new global::System.Globalization.CultureInfo(""cs-CZ"", true)", this.Serialize( ci ).ToString() );
        }

        [Fact]
        public void TestSlovakFalse()
        {
            var ci = new CultureInfo( "sk-SK", false );
            Assert.Equal( @"new global::System.Globalization.CultureInfo(""sk-SK"", false)", this.Serialize( ci ).ToString() );
        }
    }
}