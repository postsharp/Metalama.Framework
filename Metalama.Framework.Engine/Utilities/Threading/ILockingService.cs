// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using Metalama.Framework.Services;
using System;

namespace Metalama.Framework.Engine.Utilities.Threading;

public interface ILockingService : IGlobalService
{
    IDisposable WithLock( string name, ILogger? logger = null );
}

internal class GlobalLockingService : ILockingService
{
    public IDisposable WithLock( string name, ILogger? logger ) => MutexHelper.WithGlobalLock( name, logger );
}