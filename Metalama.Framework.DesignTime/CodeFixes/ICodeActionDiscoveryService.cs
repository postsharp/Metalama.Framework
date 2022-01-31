// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime.CodeFixes;

public interface ICodeActionDiscoveryService : IService
{
    Task<ComputeRefactoringResult> ComputeRefactoringsAsync(
        string projectId,
        string syntaxTreePath,
        TextSpan span,
        CancellationToken cancellationToken = default );
}