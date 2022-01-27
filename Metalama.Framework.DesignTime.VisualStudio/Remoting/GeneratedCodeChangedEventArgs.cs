// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.DesignTime.Remoting;

internal class GeneratedCodeChangedEventArgs : EventArgs
{
    public string ProjectId { get; }

    public ImmutableDictionary<string, string> GeneratedSources { get; }

    public GeneratedCodeChangedEventArgs( string projectId, ImmutableDictionary<string, string> generatedSources )
    {
        this.ProjectId = projectId;
        this.GeneratedSources = generatedSources;
    }
}