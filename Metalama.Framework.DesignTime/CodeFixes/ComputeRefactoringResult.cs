// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Newtonsoft.Json;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeFixes;

[JsonObject]
public sealed class ComputeRefactoringResult
{
    internal ImmutableArray<CodeActionBaseModel> CodeActions { get; }

    [JsonConstructor]
    public ComputeRefactoringResult( ImmutableArray<CodeActionBaseModel> codeActions )
    {
        this.CodeActions = codeActions;
    }

    public static ComputeRefactoringResult Empty { get; } = new( ImmutableArray<CodeActionBaseModel>.Empty );
}