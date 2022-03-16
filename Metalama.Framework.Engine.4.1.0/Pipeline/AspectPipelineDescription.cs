// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.Engine.Pipeline
{
    internal record AspectPipelineDescription( IExecutionScenario ExecutionScenario, bool IsTest ) : IService;
}