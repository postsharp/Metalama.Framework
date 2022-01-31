// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.CodeFixes;

public interface ICodeActionExecutionService : IService
{
    Task<CodeActionResult> ExecuteCodeActionAsync( string projectId, CodeActionModel codeActionModel, CancellationToken cancellationToken );
}