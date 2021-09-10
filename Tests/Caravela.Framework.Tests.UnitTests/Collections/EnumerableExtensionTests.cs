// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Collections;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Collections
{
    public class EnumerableExtensionTests
    {
        [Fact]
        public void AddRange()
        {
            IList<int> l = new List<int>();
            l.AddRange( new[] { 1, 2, 3 } );
            Assert.Equal( 3, l.Count );
        }
    }
}