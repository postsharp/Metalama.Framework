// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Metalama.Framework.Engine.Configuration;

// The file name has to be kept consistent with Metalama.DotNetTools.Commands.DesignTime.EditDesignTimeConfigurationCommand command class.
[ConfigurationFile( "designTime.json" )]
[Description("Options that influence the design-time behavior (such as code actions).")]
public sealed record DesignTimeConfiguration : ConfigurationFile
{
    [JsonProperty( "hideUnlicensedCodeActions" )]
    public bool HideUnlicensedCodeActions { get; init; }
}