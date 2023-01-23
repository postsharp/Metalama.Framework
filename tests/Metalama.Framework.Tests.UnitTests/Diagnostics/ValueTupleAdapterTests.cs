// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Diagnostics
{
    public sealed class ValueTupleAdapterTests
    {
        [Fact]
        public void TestValueTupleAdapter()
        {
            Assert.Equal( new object[] { 1, "2" }, ValueTupleAdapter.ToArray( (1, "2") ) );
        }
    }
}