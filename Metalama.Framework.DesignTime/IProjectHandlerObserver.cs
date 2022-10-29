﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime;

public interface IProjectHandlerObserver : IService
{
    void OnGeneratedCodePublished( ProjectKey projectKey, ImmutableDictionary<string, string> sources );
}