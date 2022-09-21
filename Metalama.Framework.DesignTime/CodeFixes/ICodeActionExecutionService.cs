// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.CodeFixes;

/// <summary>
/// A cross-process interface implemented by the analysis process that allows the user process to execute a code action.
/// </summary>
public interface ICodeActionExecutionService : IService
{
    Task<CodeActionResult> ExecuteCodeActionAsync( ProjectKey projectKey, CodeActionModel codeActionModel, bool computingPreview, CancellationToken cancellationToken );
}