// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime.CodeFixes;

/// <summary>
/// A cross-process service implemented by the analysis process that allows the user process to discover which code refactorings are present at a specified point of text.  
/// </summary>
public interface ICodeRefactoringDiscoveryService : IGlobalService, IRpcApi
{
    /// <summary>
    /// Returns the code refactorings present at a specific point of text.
    /// </summary>
    Task<ComputeRefactoringResult> ComputeRefactoringsAsync(
        ProjectKey projectKey,
        string syntaxTreePath,
        TextSpan span,
        CancellationToken cancellationToken = default );
}