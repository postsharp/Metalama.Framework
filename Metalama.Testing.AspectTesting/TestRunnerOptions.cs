// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;

namespace Metalama.Testing.AspectTesting;

[ConfigurationFile("testRunner.json")]
public record TestRunnerOptions : ConfigurationFile
{
    public bool OpenDiffTool { get; init; } = true;
}