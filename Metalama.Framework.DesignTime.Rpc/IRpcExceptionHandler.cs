﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;

namespace Metalama.Framework.DesignTime.Rpc;

public interface IRpcExceptionHandler
{
    void OnException( Exception e, ILogger logger );
}