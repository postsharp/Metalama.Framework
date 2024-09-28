// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Utilities.Threading;

internal sealed class RandomizingSingleThreadedTaskRunner : SingleThreadedTaskRunner
{
    private readonly int _seed;

    public RandomizingSingleThreadedTaskRunner( IServiceProvider serviceProvider )
    {
        var logger = serviceProvider.GetLoggerFactory().GetLogger( "TaskScheduler" );
        var seedGenerator = new Random();
        this._seed = seedGenerator.Next();

        logger.Trace?.Log( $"The random seed is '{this._seed}'." );
    }

    // ReSharper disable once UnusedMember.Global
    public RandomizingSingleThreadedTaskRunner( int seed )
    {
        this._seed = seed;
    }

    protected override IEnumerable<T> GetOrderedItems<T>( IEnumerable<T> items )
    {
        var random = new Random( this._seed );

        return items.Select( x => (Value: x, Order: random.NextDouble()) ).OrderBy( p => p.Order ).Select( p => p.Value );
    }
}