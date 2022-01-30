// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes;

public interface ICodeActionExecutionService : IService
{
    Task<CodeActionResult> ExecuteAsync( CodeActionModel codeActionModel, CancellationToken cancellationToken );
}