// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.UnitTests.DesignTimePipeline;

internal class TestDesignTimePipelineObserver : IDesignTimeAspectPipelineObserver
{
    public List<string> InitializePipelineEvents { get; } = new();

    public void OnInitializePipeline( Compilation compilation ) => this.InitializePipelineEvents.Add( compilation.AssemblyName! );
}