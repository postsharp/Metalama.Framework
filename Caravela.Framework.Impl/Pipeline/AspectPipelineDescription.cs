// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Project;

namespace Caravela.Framework.Impl.Pipeline
{
    internal record AspectPipelineDescription( AspectExecutionScenario ExecutionScenario, bool IsTest ) : IService;
}