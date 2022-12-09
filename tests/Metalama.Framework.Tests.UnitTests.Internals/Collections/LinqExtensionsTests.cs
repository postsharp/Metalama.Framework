// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Collections;

public class LinqExtensionsTests
{
    [Fact]
    public void ImmutableArrayMin()
    {
        Assert.Equal( 1, ImmutableArray.Create( 1, 2, 3 ).Min() );
        Assert.Equal( 1, ImmutableArray.Create( 1 ).Min() );
        Assert.Equal( 0, ImmutableArray<int>.Empty.Min() );
    }

    [Fact]
    public void ImmutableArrayMax()
    {
        Assert.Equal( 3, ImmutableArray.Create( 1, 2, 3 ).Max() );
        Assert.Equal( 1, ImmutableArray.Create( 1 ).Max() );
        Assert.Equal( 0, ImmutableArray<int>.Empty.Max() );
    }
}