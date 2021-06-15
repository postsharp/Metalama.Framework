// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Impl.Pipeline
{
    public record AspectPipelineDescription ( AspectExecutionScenario ExecutionScenario, bool IsTest ) : IService;
}