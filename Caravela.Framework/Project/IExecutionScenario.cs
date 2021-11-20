// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Project
{
    /// <summary>
    /// Exposes the properties of the scenarios in which an aspect or a template can be executed.
    /// </summary>
    [CompileTimeOnly]
    public interface IExecutionScenario
    {
        bool IsDesignTime { get; }

        bool SupportsNonObservableTransformations { get; }

        bool SupportsCodeFixes { get; }
    }
}