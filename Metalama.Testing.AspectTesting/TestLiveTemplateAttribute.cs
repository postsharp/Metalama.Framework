// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Testing.AspectTesting;

/// <summary>
/// A custom that must be used in the <see cref="TestScenario.ApplyLiveTemplate"/> and <see cref="TestScenario.PreviewLiveTemplate"/>
/// to mark the declaration to which the aspect must be applied. The presence of this attribute simulates the use of the refactoring
/// context menu.
/// </summary>
[AttributeUsage( AttributeTargets.All )]
public class TestLiveTemplateAttribute : Attribute
{
    public TestLiveTemplateAttribute( Type aspectType ) { }
}