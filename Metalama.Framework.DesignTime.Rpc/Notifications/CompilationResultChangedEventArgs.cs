// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Newtonsoft.Json;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Rpc.Notifications;

[JsonObject]
public class CompilationResultChangedEventArgs
{
    public ProjectKey ProjectKey { get; }

    public bool IsPartialCompilation { get; }

    public ImmutableArray<string> SyntaxTreePaths { get; }

    public CompilationResultChangedEventArgs( ProjectKey projectKey, bool isPartialCompilation, ImmutableArray<string> syntaxTreePaths )
    {
        this.ProjectKey = projectKey;
        this.IsPartialCompilation = isPartialCompilation;
        this.SyntaxTreePaths = syntaxTreePaths.IsDefault ? ImmutableArray<string>.Empty : syntaxTreePaths;
    }
}