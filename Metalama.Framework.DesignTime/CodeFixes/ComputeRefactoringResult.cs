// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeFixes;

public class ComputeRefactoringResult
{
    public ImmutableArray<CodeActionBaseModel> CodeActions { get; }

    public ComputeRefactoringResult( ImmutableArray<CodeActionBaseModel> codeActions )
    {
        this.CodeActions = codeActions;
    }

    public static ComputeRefactoringResult Empty { get; } = new( ImmutableArray<CodeActionBaseModel>.Empty );
}