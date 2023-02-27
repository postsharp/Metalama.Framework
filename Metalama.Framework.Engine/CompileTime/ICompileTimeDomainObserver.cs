﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.CompileTime;

public interface ICompileTimeDomainObserver : IGlobalService
{
    void OnDomainCreated( CompileTimeDomain domain );

    void OnDomainUnloaded( CompileTimeDomain domain );
}