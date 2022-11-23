﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Threading;

public enum TaskSchedulerKind
{
    Default,
    Concurrent = Default,
    SingleThreaded,
    RandomizingSingleThreaded
}

public interface ITaskScheduler : IProjectService // Must be project-scoped because the option to enable/disable concurrency is in the project options.
{
    Task RunInParallelAsync<T>( IEnumerable<T> items, Action<T> action, CancellationToken cancellationToken )
        where T : notnull;
}