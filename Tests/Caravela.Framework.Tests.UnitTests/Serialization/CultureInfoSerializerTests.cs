// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Globalization;
using Caravela.Framework.Impl.Serialization;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class CultureInfoSerializerTests
    {
        [Fact]
        public void TestCzech()
        {
            var serializer = new CultureInfoSerializer();
            var ci = new CultureInfo( "cs-CZ", true );
            Assert.Equal( @"new System.Globalization.CultureInfo(""cs-CZ"", true)", serializer.Serialize( ci ).ToString() );
        }

        [Fact]
        public void TestSlovakFalse()
        {
            var serializer = new CultureInfoSerializer();
            var ci = new CultureInfo( "sk-SK", false );
            Assert.Equal( @"new System.Globalization.CultureInfo(""sk-SK"", false)", serializer.Serialize( ci ).ToString() );
        }
    }
}