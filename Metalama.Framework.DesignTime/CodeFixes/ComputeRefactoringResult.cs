// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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