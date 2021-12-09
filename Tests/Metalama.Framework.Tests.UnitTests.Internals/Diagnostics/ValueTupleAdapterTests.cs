// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Diagnostics
{
    public class ValueTupleAdapterTests
    {
        [Fact]
        public void TestValueTupleAdapter()
        {
            Assert.Equal( new object[] { 1, "2" }, ValueTupleAdapter.ToArray( (1, "2") ) );
        }
    }
}