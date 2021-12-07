// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Diagnostics;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Diagnostics
{
    public class NonLocalizableStringTests
    {
        [Fact]
        public void Equal()
        {
            var s1 = new NonLocalizedString( "{0} {1}", new object[] { 1, Math.E } );
            var s2 = new NonLocalizedString( "{0} {1}", new object[] { 1, Math.E } );

            Assert.Equal( s1.GetHashCode(), s2.GetHashCode() );
            Assert.Equal( s1, s2 );
        }

        [Fact]
        public void NotEqual()
        {
            var s1 = new NonLocalizedString( "{0} {1}", new object[] { 1, Math.E } );
            var s2 = new NonLocalizedString( "{0} {1}", new object[] { 1, Math.PI } );

            Assert.NotEqual( s1.GetHashCode(), s2.GetHashCode() );
            Assert.NotEqual( s1, s2 );
        }
    }
}