﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Utilities;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public sealed class TaskBagTests
{
    [Fact]
    public async Task NonYielding()
    {
        var bag = new TaskBag( NullLogger.Instance );

        for ( var i = 0; i < 1000; i++ )
        {
            bag.Run( () => Task.CompletedTask );
        }

        await bag.WaitAllAsync();

        // This is to test that there is no memory leak.
        Assert.True( bag.IsEmpty );
    }

    [Fact]
    public async Task Yielding()
    {
        var bag = new TaskBag( NullLogger.Instance );

        for ( var i = 0; i < 1000; i++ )
        {
            bag.Run( async () => await Task.Yield() );
        }

        await bag.WaitAllAsync();

        // This is to test that there is no memory leak.
        Assert.True( bag.IsEmpty );
    }
}